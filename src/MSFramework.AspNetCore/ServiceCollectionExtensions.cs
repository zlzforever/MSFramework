using System;
using System.Text.Json;
using MicroserviceFramework.AspNetCore.DependencyInjection;
using MicroserviceFramework.AspNetCore.Infrastructure;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAspNetCore(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
			builder.Services.AddSingleton<IScopedServiceResolver, ScopedServiceResolver>();
			builder.Services.TryAddScoped<ISession, HttpSession>();

			builder.Services.AddSingleton<JsonSerializerOptions>(x =>
			{
				var jsonOptionsType =
					Type.GetType("Microsoft.AspNetCore.Mvc.JsonOptions, Microsoft.AspNetCore.Mvc.Core");
				if (jsonOptionsType == null)
				{
					throw new MicroserviceFrameworkException("Type Microsoft.AspNetCore.Mvc.JsonOptions is missing");
				}

				var type = typeof(IOptions<>).MakeGenericType(jsonOptionsType);

				return ((dynamic) x.GetRequiredService(type)).Value.JsonSerializerOptions;
			});
			return builder;
		}
		
		public static void UseMicroserviceFramework(this IApplicationBuilder builder)
		{
			builder.ApplicationServices.UseMicroserviceFramework();
		}

		public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
		{
			builder.ConfigureApiBehaviorOptions(x =>
			{
				x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
			});
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseAssemblyScanPrefix(this MicroserviceFrameworkBuilder builder,
			params string[] prefixes)
		{
			RuntimeUtilities.StartsWith.AddRange(prefixes);
			return builder;
		}
	}
}