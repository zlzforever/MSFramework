using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test.Extensions;

public class ConfigExtensionTest(ITestOutputHelper output)
{
    [Fact]
    public async Task AddConfigModel()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(builder =>
                    {
                        //
                        builder.AddJsonFile("appsettings.json");
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddMvc();
                        services.AddMicroserviceFramework(x =>
                        {
                            //
                            x.UseOptionsType(context.Configuration);
                        });
                        services.AddRouting(x => { x.LowercaseUrls = true; });
                        services.AddMicroserviceFramework(builder => { builder.UseAspNetCore(); });
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/",
                                async context => { await context.Response.WriteAsync("Hello World!"); });
                        });

                        app.UseMicroserviceFramework();
                    });
            })
            .StartAsync();

        output.WriteLine("server is running");
        var options = host.Services.GetService<IOptions<TestConfigModel>>();
        Assert.NotNull(options);
        Assert.Equal("joe", options.Value.Name);
        Assert.Equal(170, options.Value.Height);
    }
}
