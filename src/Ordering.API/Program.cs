using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Ordering.API
{
	public class Event1 : EventBase
	{
		public int Order { get; set; }

		public Event1(int i)
		{
			Order = i;
		}
	}

	public class Event1Handler : IEventHandler<Event1>
	{
		public static List<int> Result = new List<int>();

		public Task HandleAsync(Event1 @event)
		{
			Result.Add(@event.Order);
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			
		}
	}

	public class Program
	{
		public static async Task Main(string[] args)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();

			serviceCollection.AddMicroserviceFramework(builder => {   });

			var provider = serviceCollection.BuildServiceProvider();
			var eventBus = provider.GetRequiredService<IEventBus>();

			for (var i = 0; i < 100; ++i)
			{
				await eventBus.PublishAsync(new Event1(i));
			}

			Thread.Sleep(1000);

			var defaultAsms = DependencyContext.Default.GetDefaultAssemblyNames()
				.Select(x => x.Name).ToList();
			defaultAsms.Sort();
			File.WriteAllText("default.txt",
				string.Join(Environment.NewLine, defaultAsms));
			var asm1 = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName().Name).ToList();
			asm1.Sort();
			File.WriteAllText("asm1.txt",
				string.Join(Environment.NewLine, asm1));
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo.RollingFile("order.log")
				.CreateLogger();
			CreateHostBuilder(args).Build().Run();
		}


		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseUrls("http://localhost:5000");
					webBuilder.UseStartup<Startup>();
				});
	}
}