using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
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

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest
{
	public class RunTest
	{
		private readonly ITestOutputHelper _output;

		public RunTest(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
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
							services.AddMicroserviceFramework(x =>
							{
								//
								x.UseOptions(context.Configuration);
							});
							services.AddRouting(x => { x.LowercaseUrls = true; });
							services.AddMicroserviceFramework(builder =>
							{
								builder.UseAspNetCore();
								builder.UseEntityFramework(x =>
								{
									//
									x.AddNpgsql<TestDataContext>(context.Configuration);
								});
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
			_output.WriteLine("server is running");

			var dbContext = host.Services.CreateScope().ServiceProvider.GetRequiredService<TestDataContext>();
			Assert.NotNull(dbContext);
		}
	}
}