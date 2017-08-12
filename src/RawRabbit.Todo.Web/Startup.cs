using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.HttpContext;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Web.SignalR;
using Serilog;

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

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddRawRabbit(new RawRabbitOptions
			{
				Plugins = p => p
					.UseGlobalExecutionId()
					.UseHttpContext()
					.UseMessageContext(ctx => new TodoContext
					{
						Source = ctx.GetHttpContext().Request.GetDisplayUrl(),
						ExecutionId = ctx.GetGlobalExecutionId(),
						SessionId = ctx.GetHttpContext().Request.Cookies[Constants.SessionCookie]
					})
					.UseAttributeRouting()
					.UseStateMachine(),
			});
			services.AddSignalR(options =>
			{
				options.Hubs.EnableDetailedErrors = true;
			});
			var serializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				ContractResolver = new SignalRContractResolver()
			});
			services.Add(new ServiceDescriptor(typeof(JsonSerializer),
				provider => serializer,
				ServiceLifetime.Transient));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.LiterateConsole()
				.CreateLogger();
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
				if (context.Request.Cookies.ContainsKey(Constants.SessionCookie))
				{
					return func();
				}
				var sessionId = Guid.NewGuid().ToString("D");
				context.Response.Cookies.Append(Constants.SessionCookie, sessionId);
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
