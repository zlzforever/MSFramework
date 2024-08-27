using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest;

public class RunTest(ITestOutputHelper output)
{
    // [Fact]
    public async Task Run_When_AddMSFramework_WithEfNpgsql()
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
                        services.AddDbContext<TestDataContext>((provider, x) =>
                        {
                            var configuration = provider.GetRequiredService<IConfiguration>();
                            x.UseNpgsql(y =>
                            {
                                y.Load(configuration.GetSection("DbContexts:0").Get<DbContextSettings>());
                            });
                        });

                        services.AddMicroserviceFramework(builder =>
                        {
                            builder.UseOptionsType(context.Configuration);
                            builder.UseAspNetCoreExtension();
                            builder.UseEntityFramework();
                        });
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

        var dbContext = host.Services.CreateScope().ServiceProvider.GetRequiredService<TestDataContext>();
        Assert.NotNull(dbContext);
    }
}
