using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Template.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logFile = Environment.GetEnvironmentVariable("LOG");
			if (string.IsNullOrEmpty(logFile))
			{
				logFile = Path.Combine(AppContext.BaseDirectory, "template.log");
			}

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo.RollingFile(logFile)
				.CreateLogger();

			CreateHostBuilder(args).Build().Run();
			Console.WriteLine("Bye");
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.UseUrls("http://+:8000");
					webBuilder.UseSerilog();
				});
	}
}