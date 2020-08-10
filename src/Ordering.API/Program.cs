using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MSFramework;
using MSFramework.Application;
using MSFramework.Shared;
using MSFramework.Utilities;
using Newtonsoft.Json;
using Ordering.Application.Commands;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Ordering.API
{
	public class Program
	{
		private static async Task SetAuthorizationHeader()
		{
			var client = new HttpClient();
			var disco = await client.GetDiscoveryDocumentAsync("http://localhost:7897");
			if (disco.IsError)
			{
				throw new MSFrameworkException($"Connect to authority failed: {disco.Error}");
			}

			var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = disco.TokenEndpoint,
				ClientId = "cerberus-client",
				ClientSecret = "secret",
				Scope = "cerberus-api cerberus-access-server-api"
			});

			if (tokenResponse.IsError)
			{
				throw new MSFrameworkException($"Request access token failed: {tokenResponse.Error}");
			}

			var httpClient = new HttpClient();
			var token = tokenResponse.AccessToken;
			httpClient.SetBearerToken(token);
			var response = await httpClient.GetAsync("http://localhost:8100/api/v1.1/api-infos?application=x");
			response.EnsureSuccessStatusCode();
		}

		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo.RollingFile("order.log")
				.CreateLogger();
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseUrls("http://localhost:5000");
					webBuilder.UseStartup<Startup>();
				});
	}
}