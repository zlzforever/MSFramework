using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Debugging;

namespace Ordering.API;

public class Program
{
    public static Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var webApplicationBuilder = CreateWebApplicationBuilder(args);
        var web = webApplicationBuilder.Build();
        web.Configure();
        SelfLog.Enable(Console.WriteLine);
        return web.RunAsync();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
    {
        var webApplicationBuilder = WebApplication.CreateBuilder(args);
        Startup.ConfigureConfiguration(webApplicationBuilder.Configuration);
        webApplicationBuilder.Host
            .ConfigureServices(Startup.ConfigureServices)
            .UseSerilog();
        webApplicationBuilder.WebHost.UseUrls("http://+:5001");
        return webApplicationBuilder;
    }
}
