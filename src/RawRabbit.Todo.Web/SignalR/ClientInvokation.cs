using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;

namespace RawRabbit.Todo.Web.SignalR
{
	public class ClientInvokation
	{
		private readonly IBusClient _busClient;
		private readonly IConnectionManager _connectionMgmt;

		public ClientInvokation(IServiceProvider provider)
		{
			_busClient = provider.GetService<IBusClient>();
			_connectionMgmt = provider.GetService<IConnectionManager>();
		}

		public async Task StartAsync()
		{
			await _busClient.SubscribeAsync<TodoCreated, TodoContext>((created, context) =>
			{
				_connectionMgmt.GetHubContext<TodoHub>().Clients.All.publishTodo(created.Todo);
				return Task.CompletedTask;
			});

			await _busClient.SubscribeAsync<NotifyClients>(notificatoin =>
			{
				_connectionMgmt.GetHubContext<TodoHub>().Clients.All.publishNotification(notificatoin);
				return Task.CompletedTask;
			});

			await _busClient.SubscribeAsync<TodoListCreated, TodoContext>((list, context) =>
			{
				_connectionMgmt.GetHubContext<TodoHub>().Clients.Group(context.SessionId).populateList(list.Todos);
				return Task.CompletedTask;
			});
		}
	}
}
