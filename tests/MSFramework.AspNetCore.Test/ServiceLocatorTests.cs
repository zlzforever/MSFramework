using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test;

public class ServiceLocatorTests
{
    private readonly ITestOutputHelper _output;

    public ServiceLocatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private class A
    {
        public string TraceIdentifier { get; }

        public A()
        {
            TraceIdentifier = ObjectId.GenerateNewId().ToString();
        }
    }

    [Fact]
    public async Task Scoped()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureAppConfiguration(builder =>
                    {
                        //
                        builder.AddJsonFile("EfPostgreSqlTest.json");
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddMvc();
                        services.AddRouting(x => { x.LowercaseUrls = true; });
                        services.AddMicroserviceFramework(builder =>
                        {
                            builder.UseOptionsType(context.Configuration);
                            builder.UseAspNetCore();
                        });
                        services.AddScoped<A>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/",
                                async context =>
                                {
                                    var session1 = context.RequestServices.GetRequiredService<A>();
                                    var session2 = ServiceLocator.GetService<A>();
                                    await context.Response.WriteAsync(
                                        session1.TraceIdentifier == session2.TraceIdentifier ? "ok" : "");
                                });
                        });

                        app.UseMicroserviceFramework();
                    });
            })
            .StartAsync();
        _output.WriteLine("server is running");

        var result = await host.GetTestClient().GetStringAsync("/");

        Assert.Equal("ok", result);
    }
}
