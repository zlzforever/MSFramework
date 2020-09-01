using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
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
		public void Run_When_AddMSFramework_WithEfNpgsql()
		{
			var hostBuilder = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					// Add TestServer
					webHost.UseTestServer().ConfigureAppConfiguration(i => { i.AddJsonFile("EfPostgreSqlTest.json"); });
					webHost.UseStartup<Startup>().ConfigureServices((context, service) =>
					{
						var config = context.Configuration;
						service.AddMicroserviceFramework(builder =>
						{
							builder.UseDependencyInjectionScanner();
							builder.UseAspNetCore();
							builder.UseEntityFramework(x => { x.AddNpgsql<TestDataContext>(config); });
						});
					}).Configure(builder => { builder.UseMicroserviceFramework(); });
				});
			var host = hostBuilder.Start();
			_output.WriteLine("server is runing");
		}
	}
}