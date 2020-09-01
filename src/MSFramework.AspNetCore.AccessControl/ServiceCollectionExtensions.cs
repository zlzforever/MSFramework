using System;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAccessControl(this MicroserviceFrameworkBuilder builder, IConfiguration configuration)
		{
			builder.Services.TryAddScoped<AccessControlOptions>();
			builder.Services.TryAddScoped<IAccessClient, AccessClient>();
			builder.Services.AddHttpClient();

			var options = new AccessControlOptions(configuration);

			if (!string.IsNullOrWhiteSpace(options.Authority))
			{
				var httpClient = new HttpClient();
				var disco = httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
				{
					Address = options.Authority,
					Policy = new DiscoveryPolicy
					{
						RequireHttps = false
					}
				}).Result;
				if (disco.TokenEndpoint == null)
				{
					throw new ApplicationException($"TokenEndpoint {options.Authority} is null");
				}

				builder.Services.AddAccessTokenManagement(x =>
				{
					x.Client.Clients.Add("default", new ClientCredentialsTokenRequest
					{
						Address = disco.TokenEndpoint,
						ClientId = options.ClientId,
						ClientSecret = options.ClientSecret
					});
					x.Client.Scope = "cerberus-api cerberus-access-server-api";
				});
				builder.Services.AddClientAccessTokenClient(options.HttpClient);
			}

			return builder;
		}
	}
}