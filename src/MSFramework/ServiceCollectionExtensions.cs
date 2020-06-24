using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MSFramework.Audit;
using MSFramework.Collections.Generic;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.Domain;
using MSFramework.Domain.Event;
using MSFramework.Reflection;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddEventMediator(this MSFrameworkBuilder builder, params Type[] eventTypes)
		{
			var excludeAssembly = typeof(MSFrameworkBuilder).Assembly;
			if (eventTypes.Any(x => x.Assembly != excludeAssembly))
			{
				var list = new List<Type>(eventTypes) {typeof(MSFrameworkBuilder)};
				builder.Services.AddEventMediator(list.ToArray());
			}
			else
			{
				builder.Services.AddEventMediator(eventTypes);
			}

			return builder;
		}

		public static void AddMSFramework(this IServiceCollection services,
			Action<MSFrameworkBuilder> builderAction = null)
		{
			var builder = new MSFrameworkBuilder(services);
			builderAction?.Invoke(builder);

			builder.AddEventMediator(typeof(MSFrameworkBuilder));

			//初始化所有程序集查找器，如需更改程序集查找逻辑，请事先赋予自定义查找器的实例
			if (Singleton<IAssemblyFinder>.Instance == null)
			{
				Singleton<IAssemblyFinder>.Instance = new AssemblyFinder();
			}

			if (Singleton<IDependencyTypeFinder>.Instance == null)
			{
				Singleton<IDependencyTypeFinder>.Instance = new DependencyTypeFinder();
				var dependencyTypeDict = Singleton<IDependencyTypeFinder>.Instance.GetDependencyTypeDict();
				foreach (var kv in dependencyTypeDict)
				{
					builder.Services.Add(kv.Value, kv.Key);
				}
			}

			if (Singleton<IIdGenerator>.Instance == null)
			{
				Singleton<IIdGenerator>.Instance = new IdGenerator();
			}

			builder.AddInitializer();

			builder.Services.TryAddScoped<ScopedDictionary>();
			builder.Services.TryAddScoped<IUnitOfWorkManager, DefaultUnitOfWorkManager>();
			// 如果你想换成消息队列，则重新注册一个对应的服务即可
			builder.Services.TryAddScoped<IAuditService, DefaultAuditService>();
		}

		public static IMSFrameworkApplicationBuilder UseMSFramework(this IServiceProvider applicationServices,
			Action<IMSFrameworkApplicationBuilder> configure = null)
		{
			InitializeAsync(applicationServices).GetAwaiter().GetResult();

			ExecuteDatabaseMigration(applicationServices);

			var builder = new MSFrameworkApplicationBuilder(applicationServices);
			configure?.Invoke(builder);
			return builder;
		}

		public static MSFrameworkBuilder AddDatabaseMigration<T>(this MSFrameworkBuilder builder, Type type,
			string connectionString) where T : DatabaseMigration
		{
			builder.Services.AddScoped<IDatabaseMigration>(provider =>
				ActivatorUtilities.CreateInstance<T>(provider, type, connectionString));
			return builder;
		}

		private static async Task InitializeAsync(IServiceProvider applicationServices)
		{
			using var scope = applicationServices.CreateScope();
			var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Initializer");
			var initializers = scope.ServiceProvider.GetServices<Initializer>().OrderBy(x => x.Order).ToList();
			logger.LogInformation($"{string.Join(", ", initializers.Select(x => x.GetType().FullName))}");
			foreach (var initializer in initializers)
			{
				await initializer.InitializeAsync(scope.ServiceProvider);
			}
		}

		private static void ExecuteDatabaseMigration(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var migrations = scope.ServiceProvider.GetServices<IDatabaseMigration>();
			foreach (var migration in migrations)
			{
				migration.Execute();
			}
		}
	}
}