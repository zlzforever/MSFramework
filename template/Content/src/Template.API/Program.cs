using System;
using System.IO;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using Serilog.Events;

namespace Template.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			DefaultTypeMap.MatchNamesWithUnderscores = true;
			CreateHostBuilder(args).Build().Run();
			Console.WriteLine("Bye");
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, builder) =>
				{
					var configuration = builder.Build();
					builder.AddAliyunJsonFile(source =>
					{
						source.Endpoint = configuration["RemoteConfiguration:Endpoint"];
						source.BucketName = configuration["RemoteConfiguration:BucketName"];
						source.AccessKeyId = configuration["RemoteConfiguration:AccessKeyId"];
						source.AccessKeySecret = configuration["RemoteConfiguration:AccessKeySecret"];
						source.Key = configuration["RemoteConfiguration:Key"];
					});

					if (File.Exists("serilog.json"))
					{
						builder.AddJsonFile("serilog.json");
						Log.Logger = new LoggerConfiguration().ReadFrom
							.Configuration(builder.Build())
							.CreateLogger();
					}
					else
					{
						var logFile = Environment.GetEnvironmentVariable("LOG");
						if (string.IsNullOrEmpty(logFile))
						{
							logFile = Path.Combine(AppContext.BaseDirectory, "logs/template.log");
						}

						Log.Logger = new LoggerConfiguration()
							.MinimumLevel.Information()
							.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
#if DEBUG
							.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
								LogEventLevel.Information)
#endif
							.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
							.MinimumLevel.Override("System", LogEventLevel.Warning)
							.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
							.Enrich.FromLogContext()
							.WriteTo.Console().WriteTo.RollingFile(logFile)
							.CreateLogger();
					}
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.ConfigureKestrel((context, options) =>
					{
						// Handle requests up to 500 MB
						options.Limits.MaxRequestBodySize = 1024288000;
						options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
						options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(20);
					});
					webBuilder.UseStartup<Startup>();
				}).UseSerilog();
	}
}