using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Initializer
{
	public static class InitializerServiceCollectionExtensions
	{
		internal static MicroserviceFrameworkBuilder UseInitializer(this MicroserviceFrameworkBuilder builder)
		{
			var initializerType = typeof(InitializerBase);
			var nonAutomaticInitializerType = typeof(INotAutomaticRegisterInitializer);
			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				if (initializerType.IsAssignableFrom(type) &&
				    !nonAutomaticInitializerType.IsAssignableFrom(type))
				{
					builder.Services.AddSingleton(initializerType, type);
				}
			};
			return builder;
		}

		public static MicroserviceFrameworkBuilder AddInitializer<TInitializer>(
			this MicroserviceFrameworkBuilder builder)
			where TInitializer : InitializerBase
		{
			builder.Services.AddSingleton<TInitializer>();
			return builder;
		}

		public static IServiceCollection AddInitializer<TInitializer>(this IServiceCollection serviceCollection)
			where TInitializer : InitializerBase
		{
			serviceCollection.AddSingleton<InitializerBase, TInitializer>();
			return serviceCollection;
		}
	}
}