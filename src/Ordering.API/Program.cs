using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Ordering.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
            				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("ordering.log");
			Log.Logger = configure.CreateLogger();

			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>().UseSerilog().UseUrls("http://0.0.0.0:5000");
	}
}