using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Options;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("MSFramework.Newtonsoft")]
[assembly: InternalsVisibleTo("MSFramework.AspNetCore")]

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseOptions(this MicroserviceFrameworkBuilder builder,
			IConfiguration configuration)
		{
			builder.Services.AddOptions(configuration);
			return builder;
		}

		public static void AddMicroserviceFramework(this IServiceCollection services,
			Action<MicroserviceFrameworkBuilder> builderAction = null)
		{
			var builder = new MicroserviceFrameworkBuilder(services);

			builder.Services.TryAddSingleton<ApplicationInfo>();
			builder.UseDefaultSerializer();

			// 放到后面，加载优先级更高
			builderAction?.Invoke(builder);

			// 请保证这在最后，不然类型扫描事件的注册会晚于扫描
			MicroserviceFrameworkLoaderContext.Get(services).LoadTypes();
		}

		public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
		{
			var configuration = applicationServices.GetService<IConfiguration>();
			if (configuration == null)
			{
				return;
			}

			var loggerFactory = applicationServices.GetService<ILoggerFactory>();
			if (loggerFactory == null)
			{
				return;
			}

			var logger = loggerFactory.CreateLogger("UseMicroserviceFramework");

			var initializers = applicationServices.GetServices<IHostedService>().Where(x => x is InitializerBase)
				.ToList();
			logger.LogInformation(
				$"Initializers: {string.Join(" -> ", initializers.Select(x => x.GetType().FullName))}");

			ServiceLocator.ServiceProvider = applicationServices;
		}
	}
}