using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MSFramework.AspNetCore.Function;
using MSFramework.AspNetCore.Infrastructure;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;
using MSFramework.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.PostgreSql;
using MSFramework.Function;
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
						service.AddMSFramework(builder =>
						{
							builder.UseDependencyInjectionScanner();
							builder.UseAspNetCore();
							builder.UseEntityFramework(x => { x.AddNpgsql<TestDataContext>(config); });
						});
					}).Configure(builder => 
					{
						builder.UseMSFramework();
					});
				});
			var host = hostBuilder.Start();
			_output.WriteLine("server is runing");
		}
	}
}