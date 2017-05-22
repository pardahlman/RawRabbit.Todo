using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RawRabbit.Common;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Subscribe;
using RawRabbit.Enrichers.QueueSuffix;
using RawRabbit.Instantiation;
using RawRabbit.Logging;
using RawRabbit.Logging.Serilog;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;

namespace RawRabbit.Todo
{
	public class Program
	{
		public static void Main(string[] args)
		{
			RunAsync()
				.GetAwaiter()
				.GetResult();
		}

		public static async Task RunAsync()
		{
			var repo = new TodoRepository();
			var busClient = RawRabbitFactory.CreateSingleton(new RawRabbitOptions
			{
				Plugins = p => p
					.UseAttributeRouting()
					.UseGlobalExecutionId()
					.UseApplicationQueueSuffix()
					.UseContextForwarding()
					.UseMessageContext<TodoContext>()
			});
			LogManager.CurrentFactory = new RawRabbit.Logging.LoggerFactory(s => new ConsoleLogger(LogLevel.Debug, s));

			await busClient.SubscribeAsync<CreateTodo, TodoContext>(async (msg, context) =>
			{
				if (msg.Todo == null)
				{
					return new Nack(false);
				}
				var created = await repo.AddAsync(msg.Todo);
				await busClient.PublishAsync(new TodoCreated
				{
					Todo = created
				}, ctx => ctx.UsePublishAcknowledge(false));
				return new Ack();
			});

			await busClient.SubscribeAsync<RemoveTodo, BasicDeliverEventArgs>(async (remove, args) =>
				{
					if (args.Redelivered)
					{
						return;
					}
					var exist = await repo.GetAsync(remove.Id);
					if (exist == null)
					{
						await busClient.PublishAsync(new RemoveTodoFailed
						{
							Message = $"Todo with id {remove.Id} not found."
						});
					}
					await repo.RemoveAsync(remove.Id);
					await busClient.PublishAsync(new TodoRemoved
					{
						Removed = exist
					});
				}, ctx => ctx
				.UseMessageContext(c => c.GetDeliveryEventArgs())
				.UseConsumerConfiguration(cfg => cfg
					.Consume(c => c
						.WithNoAck()
						.WithPrefetchCount(1))
					.FromDeclaredQueue(q => q
						.WithName("remove_todo")
						.WithAutoDelete()
						.WithArgument(QueueArgument.DeadLetterExchange, "dlx")
					))
			);

			await busClient.BasicConsumeAsync(async args =>
			{
				var created = JsonConvert.DeserializeObject<TodoCreated>(Encoding.UTF8.GetString(args.Body));
				var allTodos = await repo.GetAllAsync();
				var newOwner = !allTodos.Any(t => t.Id != created.Todo.Id && t.Owner == created.Todo.Owner);
				if (newOwner)
				{
					await busClient.PublishAsync(new NotifyClients
					{
						Message = $"First todo for owner {created.Todo.Owner}",
						Time = DateTime.Now
					}, ctx => ctx.UsePublishAcknowledge(false));
				}
				return new Ack();
			}, ctx => ctx
				.UseConsumerConfiguration(cfg => cfg
					.Consume(c => c
						.WithRoutingKey("todocreated.#")
						.OnExchange("rawrabbit.todo.shared.messages")
					)
					.FromDeclaredQueue(q => q
						.WithName("new_owner_evaluation")
					))
			);

			await busClient.RespondAsync<TodoRequest, TodoResponse>(async req =>
			{
				var todo = await repo.GetAsync(req.Id);
				return new TodoResponse { Todo = todo };
			});

			await busClient.SubscribeAsync<CreateTodoList, TodoContext>(async (msg, context) =>
			{
				if (msg.Count == int.MaxValue)
				{
					var todos = await repo.GetAllAsync();
					await busClient.PublishAsync(new TodoListCreated() { Todos = todos });
					return new Ack();
				}
				return new Nack(false);
			});

			Console.ReadKey();
		}
	}
}
