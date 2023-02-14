using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ordering.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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
        return webApplicationBuilder;
    }
}
