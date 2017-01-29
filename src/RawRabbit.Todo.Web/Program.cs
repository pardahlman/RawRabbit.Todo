using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;
using RawRabbit.Todo.Shared.Repo;
using RawRabbit.Todo.Web.SignalR;

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

			new ClientInvokation(host.Services)
				.StartAsync()
				.GetAwaiter()
				.GetResult();

			host.Run();
		}
	}
}
