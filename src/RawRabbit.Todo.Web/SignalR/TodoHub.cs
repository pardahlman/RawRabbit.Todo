using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RawRabbit.Todo.Web.Controllers;

namespace RawRabbit.Todo.Web.SignalR
{
	public class TodoHub : Hub
	{
		public override Task OnConnected()
		{
			string cookie;
			if (Context.Request.Cookies.TryGetValue(BaseController.SessionCookie, out cookie))
			{
				Groups.Add(Context.ConnectionId, cookie);
			}
			return base.OnConnected();
		}
	}
}
