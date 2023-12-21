using System;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Template.API;

public static class Program
{
    public static Task Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var webApplicationBuilder = CreateWebApplicationBuilder(args);
        var web = webApplicationBuilder.Build();
        web.Configure();
        return web.RunAsync();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(x =>
            {
                x.AddEnvironmentVariables();
                x.AddCommandLine(args);
            })
            .ConfigureAppConfiguration()
            .ConfigureServices();
        var webApplicationBuilder = WebApplication.CreateBuilder(args);


        webApplicationBuilder.WebHost.ConfigureKestrel((_, options) =>
        {
            // Handle requests up to 500 MB
            options.Limits.MaxRequestBodySize = 1024288000;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(20);
        });
        return webApplicationBuilder;
    }
}
