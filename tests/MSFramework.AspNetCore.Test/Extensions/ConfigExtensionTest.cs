using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MSFramework.AspNetCore.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test.Extensions
{
	public class ConfigExtensionTest
	{
		private readonly ITestOutputHelper _output;

		public ConfigExtensionTest(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void AddConfigModel()
		{
			var hostBuilder = new HostBuilder()
				.ConfigureWebHost(webHost =>
				{
					// Add TestServer
					webHost.UseTestServer().ConfigureAppConfiguration(i => { i.AddJsonFile("appsettings.json"); });
					webHost.UseStartup<Startup>().ConfigureServices((context, service) =>
					{
						service.AddConfigType(GetType().Assembly);
					}).Configure(builder => { builder.UseMSFramework(); });
				});
			var host = hostBuilder.Start();
			_output.WriteLine("server is runing");
			var testConfigModel = (TestConfigModel) host.Services.GetService(typeof(TestConfigModel));
			Assert.Equal("joe", testConfigModel.Name);
			Assert.Equal(170, testConfigModel.Height);
		}
	}
}