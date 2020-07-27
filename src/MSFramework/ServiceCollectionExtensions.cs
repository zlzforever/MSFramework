using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Audit;
using MSFramework.Domain;
using MSFramework.Domain.Event;
using MSFramework.Initializer;
using MSFramework.Reflection;

namespace MSFramework
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseEventDispatcher(this MSFrameworkBuilder builder)
		{
			var assemblies = AssemblyFinder.GetAllList();
			builder.UseEventDispatcher(assemblies.ToArray());
			return builder;
		}

		public static MSFrameworkBuilder UseRequestExecutor(this MSFrameworkBuilder builder)
		{
			var assemblies = AssemblyFinder.GetAllList();
			builder.UseRequestExecutor(assemblies.ToArray());
			return builder;
		}

		public static MSFrameworkBuilder UseRequestExecutor(this MSFrameworkBuilder builder, params Type[] commandTypes)
		{
			var excludeAssembly = typeof(MSFrameworkBuilder).Assembly;
			var assemblies = commandTypes.Select(x => x.Assembly).ToList();

			if (!assemblies.Contains(excludeAssembly))
			{
				assemblies.Add(excludeAssembly);
			}

			builder.UseRequestExecutor(assemblies.ToArray());
			return builder;
		}

		public static MSFrameworkBuilder UseEventDispatcher(this MSFrameworkBuilder builder, params Type[] eventTypes)
		{
			var excludeAssembly = typeof(MSFrameworkBuilder).Assembly;
			var assemblies = eventTypes.Select(x => x.Assembly).ToList();

			if (!assemblies.Contains(excludeAssembly))
			{
				assemblies.Add(excludeAssembly);
			}

			builder.UseEventDispatcher(assemblies.ToArray());
			return builder;
		}

		public static MSFrameworkBuilder UseEventDispatcher(this MSFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddEventDispatcher(assemblies);
			return builder;
		}

		public static MSFrameworkBuilder UseRequestExecutor(this MSFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddRequestExecutor(assemblies);
			return builder;
		}

		public static void AddMSFramework(this IServiceCollection services,
			Action<MSFrameworkBuilder> builderAction = null)
		{
			var builder = new MSFrameworkBuilder(services);
			builderAction?.Invoke(builder);

			services.AddMemoryCache();

			builder.Services.TryAddScoped<IUnitOfWorkManager, DefaultUnitOfWorkManager>();

			// 如果你想换成消息队列，则重新注册一个对应的服务即可
			builder.Services.TryAddScoped<IAuditService, DefaultAuditService>();

			builder.UseInitializer();
		}

		public static void UseMSFramework(this IServiceProvider applicationServices)
		{
			InitializeAsync(applicationServices).GetAwaiter().GetResult();
		}

		private static async Task InitializeAsync(IServiceProvider applicationServices)
		{
			using var scope = applicationServices.CreateScope();
			var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Initializer");
			var initializers = scope.ServiceProvider.GetServices<InitializerBase>().OrderBy(x => x.Order).ToList();
			logger.LogInformation($"{string.Join(", ", initializers.Select(x => x.GetType().FullName))}");
			foreach (var initializer in initializers)
			{
				await initializer.InitializeAsync(scope.ServiceProvider);
			}
		}
	}
}