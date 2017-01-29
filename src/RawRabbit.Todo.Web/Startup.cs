using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Operations.StateMachine;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Repo;
using RawRabbit.Todo.Web.Controllers;
using RawRabbit.Todo.Web.SignalR;
using RawRabbit.vNext;
using RawRabbit.vNext.Pipe;

namespace RawRabbit.Todo.Web
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
			services.AddRawRabbit(new RawRabbitOptions
			{
				Plugins = p => p
					.UseMessageContext<TodoContext>()
					.UseStateMachine()
			});
			services.AddSignalR(options =>
			{
				options.Hubs.EnableDetailedErrors = true;
			});
			services.AddSingleton<ITodoRepository, TodoRepository>();
			var serializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				ContractResolver = new SignalRContractResolver()
			});
			services.Add(new ServiceDescriptor(typeof(JsonSerializer),
				provider => serializer,
				ServiceLifetime.Transient));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.Use((context, func) =>
			{
				if (context.Request.Cookies.ContainsKey(BaseController.SessionCookie))
				{
					return func();
				}
				var sessionId = Guid.NewGuid().ToString("D");
				context.Response.Cookies.Append(BaseController.SessionCookie, sessionId);
				return func();
			});
			app.UseDefaultFiles();
			app.UseStaticFiles();


			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseWebSockets();
			app.UseSignalR();
		}
	}
}
