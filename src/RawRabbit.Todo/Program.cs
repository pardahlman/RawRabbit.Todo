using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Subscribe;
using RawRabbit.Instantiation;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;
using RawRabbit.Todo.Shared.Repo;

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
					.UseContextForwaring()
					.UseMessageContext<TodoContext>()
			});

			Console.WriteLine("Subscribing to Todo");
			await busClient.SubscribeAsync<CreateTodo, TodoContext>(async (msg, context) =>
			{
				if (msg.Todo == null)
				{
					return new Nack(false);
				}
				var created = await repo.AddAsync(msg.Todo);
				await busClient.PublishAsync(new TodoCreated {Todo = created});
				return new Ack();
			}, ctx => ctx.UseConsumerRecovery(TimeSpan.FromHours(1)));

			await busClient.SubscribeAsync<CreateTodo>(async (created) =>
				{
					var allTodos = await repo.GetAllAsync();
					var newOwner = allTodos.All(t => t.Owner != created.Todo.Owner);
					if (newOwner)
					{
						await busClient.PublishAsync(new NotifyClients
						{
							Message = $"First todo for owner {created.Todo.Owner}",
							Time = DateTime.Now
						});
					}
				}, ctx => ctx
				.UseMessageContext(c => c.GetDeliveryEventArgs())
				.UseHostnameQueueSuffix()
				.UseConsumerConfiguration(cfg => cfg
					.Consume(c => c
						.WithNoAck()
						.WithPrefetchCount(1))
					.FromDeclaredQueue(q => q
						.WithName("todo_owner")
						.WithAutoDelete()
						.WithArgument(QueueArgument.DeadLetterExchange, "dlx")
					))
			);

			Console.WriteLine("Subscribing to Todo RPC");

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

			Console.WriteLine("Done with it all.");
			Console.ReadKey();
		}
	}
}
