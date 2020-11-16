using System;
using System.Net.Http;
using Cerberus.AspNetCore.AccessControl;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAccessControl(this MicroserviceFrameworkBuilder builder,
			IConfiguration configuration)
		{
			builder.Services.AddAccessControl(configuration);
			return builder;
		}
	}
}