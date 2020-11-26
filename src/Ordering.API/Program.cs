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
		public static void Main(string[] args)
		{
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