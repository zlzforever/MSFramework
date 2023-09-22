using System;
using System.Text;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Ordering.Application.Events;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

namespace Ordering.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var date1 = new DateTime(2023, 1, 1);
        var date2 = new DateTime(2023, 6, 7);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var webApplicationBuilder = CreateWebApplicationBuilder(args);
        var web = webApplicationBuilder.Build();

        // web.Map("OnProductCreated", [Topic("pubsub", Names.OrderCreatedEvent)](E e) =>
        // {
        //     var a = Defaults.JsonSerializer.Serialize(e);
        //     Log.Logger.Information(a);
        // });

        web.Configure();
        await web.RunAsync();
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

    public class E
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long CreationTime { get; set; }
    }
}
