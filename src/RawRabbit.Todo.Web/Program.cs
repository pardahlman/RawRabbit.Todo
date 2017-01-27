using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;
using RawRabbit.Todo.Shared.Repo;
using RawRabbit.Todo.Web.Hubs;

namespace RawRabbit.Todo.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build();

			var busClient = host.Services.GetService<IBusClient>();
			var connectionMgmt = host.Services.GetService<IConnectionManager>();
			busClient.SubscribeAsync<TodoCreated, TodoContext>((created, context) =>
			{
				connectionMgmt.GetHubContext<TodoHub>().Clients.All.publishTodo(created.Todo);
				return Task.CompletedTask;
			});

			busClient.SubscribeAsync<NotifyClients>(notificatoin =>
			{
				connectionMgmt.GetHubContext<TodoHub>().Clients.All.publishNotification(notificatoin);
				return Task.CompletedTask;
			});

			busClient.SubscribeAsync<TodoListCreated, TodoContext>((list, context) =>
			{
				connectionMgmt.GetHubContext<TodoHub>().Clients.Group(context.SessionId).populateList(list.Todos);
				return Task.CompletedTask;
			});

			host.Run();
		}
	}
}
