using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test
{
	public class ServiceLocatorTests
	{
		private readonly ITestOutputHelper _output;

		public ServiceLocatorTests(ITestOutputHelper output)
		{
			_output = output;
		}

		class A
		{
			public string TraceIdentifier { get; }

			public A()
			{
				TraceIdentifier = ObjectId.NewId().ToString();
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
								builder.UseOptions(context.Configuration);
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
}