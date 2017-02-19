using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace RawRabbit.Todo.Web.SignalR
{
	public class TodoHub : Hub
	{
		public override Task OnConnected()
		{
			string cookie;
			if (Context.Request.Cookies.TryGetValue(Constants.SessionCookie, out cookie))
			{
				Groups.Add(Context.ConnectionId, cookie);
			}
			return base.OnConnected();
		}
	}
}
