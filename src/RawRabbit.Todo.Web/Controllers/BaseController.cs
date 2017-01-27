using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Pipe;
using RawRabbit.Todo.Shared;

namespace RawRabbit.Todo.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		protected readonly IBusClient BusClient;
		public const string SessionCookie = "rawrabbit:sessionid";

		protected BaseController(IBusClient busClient)
		{
			BusClient = busClient;
		}

		protected Task PublishAsync<TMessage>(TMessage message = default(TMessage), Action<IPipeContext> context = null)
		{
			context = context ?? (pipeContext => { });
			context += c => c.UseMessageContext(CreateTodoContext());
			return BusClient.PublishAsync(message, context);
		}

		private TodoContext CreateTodoContext()
		{
			return new TodoContext
			{
				Source = Request.Headers.ContainsKey("Referer") ? Request.Headers["Referer"].ToString() : Request.GetDisplayUrl(),
				ExecutionId = Guid.NewGuid(),
				SessionId = GetSessionId()
			};
		}

		private string GetSessionId()
		{
			if (Request.Cookies.ContainsKey(SessionCookie))
			{
				return Request.Cookies[SessionCookie];
			}
			var sessionId = Guid.NewGuid().ToString("D");
			Response.Cookies.Append(SessionCookie, sessionId);
			return sessionId;
		}
	}
}
