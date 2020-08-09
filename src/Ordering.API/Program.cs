using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MSFramework.Application;
using MSFramework.Shared;
using MSFramework.Utilities;
using Newtonsoft.Json;
using Ordering.Application.Commands;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Ordering.API
{
	public class Program
	{
		struct MyStruct
		{
			private int a;
		}

		public static void Main(string[] args)
		{
			var id1 = Guid.NewGuid();
			var j1 = JsonConvert.SerializeObject(id1);
			var id2 = new MyStruct();
			var j2 = JsonConvert.SerializeObject(id2);

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