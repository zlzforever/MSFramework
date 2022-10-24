using System;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Template.API;

public class Program
{
	public static async Task Main(string[] args)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		DefaultTypeMap.MatchNamesWithUnderscores = true;

		var webApplicationBuilder = CreateWebApplicationBuilder(args);
		var web = webApplicationBuilder.Build();
		web.Configure();
		await web.RunAsync();
	}

	public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
	{
		var webApplicationBuilder = WebApplication.CreateBuilder(args);
		webApplicationBuilder.Host.ConfigureAppConfiguration(Startup.ConfigureConfiguration)
			.ConfigureServices(Startup.ConfigureServices)
			.UseSerilog();
		webApplicationBuilder.WebHost.UseUrls("http://+:5001");
		webApplicationBuilder.WebHost.ConfigureKestrel((context, options) =>
		{
			// Handle requests up to 500 MB
			options.Limits.MaxRequestBodySize = 1024288000;
			options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
			options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(20);
		});
		return webApplicationBuilder;
	}
}