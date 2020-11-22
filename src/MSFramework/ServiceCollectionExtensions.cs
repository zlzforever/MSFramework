using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MicroserviceFramework.Application;
using MicroserviceFramework.Audit;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Initializer;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseEventBus(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.AddEventBus();
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
			builderAction?.Invoke(builder);

			services.AddSerializer();

			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				var lifetime = LifetimeChecker.Get(type);
				if (lifetime.HasValue)
				{
					builder.Services.RegisterDependencyInjection(type, lifetime.Value);
				}
			};

			builder.Services.TryAddScoped<UnitOfWorkManager>();

			// 如果你想换成消息队列，则重新注册一个对应的服务即可
			builder.Services.TryAddScoped<IAuditService, DefaultAuditService>();

			services.AddDomainEventDispatcher();

			MicroserviceFrameworkLoader.RegisterTypes();

			builder.UseInitializer();
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
			applicationServices.UseInitializerAsync().GetAwaiter().GetResult();
		}
	}
}