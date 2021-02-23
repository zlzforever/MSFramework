using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MicroserviceFramework.Application;
using MicroserviceFramework.Audit;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Initializer;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicroserviceFramework.Configuration;

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

		public static MicroserviceFrameworkBuilder UseCqrs(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.AddCqrs();
			return builder;
		}

		public static void AddMicroserviceFramework(this IServiceCollection services,
			Action<MicroserviceFrameworkBuilder> builderAction = null)
		{
			var builder = new MicroserviceFrameworkBuilder(services);
			
			builder.Services.AddDependencyInjectionLoader();
			builder.Services.AddDomainEvent();
			builder.Services.AddSerializer();
			builder.Services.AddEventBus();
			builder.Services.AddInitializer();
			// 如果你想换成消息队列，则重新注册一个对应的服务即可
			builder.Services.TryAddScoped<IAuditService, DefaultAuditService>();
			builder.Services.TryAddSingleton<ApplicationInfo>();

			ObjectId.AddTypeDescriptor();

			// 放到后面，加载优先级更高
			builderAction?.Invoke(builder);
			
			// 请保证这在最后，不然类型扫描事件的注册会晚于扫描
			MicroserviceFrameworkLoaderContext.Default.LoadTypes();
		}

		public static MicroserviceFrameworkBuilder UseSerializer(this MicroserviceFrameworkBuilder builder,
			Action<JsonSerializerOptions> configure = null)
		{
			builder.Services.AddSerializer(configure);
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseBaseX(this MicroserviceFrameworkBuilder builder,
			string path = "basex.txt")
		{
			string codes;
			if (!File.Exists(path))
			{
				codes = BaseX.GetRandomCodes();
				File.WriteAllText(path, codes);
			}
			else
			{
				codes = File.ReadAllLines(path).FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(codes) || codes.Length < 34)
			{
				throw new ArgumentException("Codes show large than 34 char");
			}

			BaseX.Load(codes);
			return builder;
		}

		public static void UseMicroserviceFramework(this IServiceProvider applicationServices)
		{
			ServiceLocator.ServiceProvider = applicationServices;

			applicationServices.UseInitializerAsync().GetAwaiter().GetResult();
		}
	}
}