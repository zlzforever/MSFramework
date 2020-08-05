using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MSFramework.Application;
using Ordering.Application.Commands;
using Serilog;
using Serilog.Events;

namespace Ordering.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
	
			
			var t1 = typeof(CreateOrderCommand);
			var t2 = typeof(ChangeOrderAddressCommand);

			var r1 = typeof(IRequest).IsAssignableFrom(t1);
			var r2 = typeof(IRequest).IsAssignableFrom(t2);
			var r3 = typeof(IRequest<>).IsAssignableFrom(t1);
			var r4 = typeof(IRequest<>).IsAssignableFrom(t2);

			var i = t1.GetInterfaces();

			var a = i[0].GetGenericTypeDefinition();
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