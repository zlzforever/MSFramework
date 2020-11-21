using System;
using System.Linq;
using System.Threading;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Application
{
	public static class ServiceCollectionExtensions
	{
	
		public static IServiceCollection AddCQRS(this IServiceCollection serviceCollection)
		{
			var handlerInterfaceTypes = new[]
			{
				typeof(ICommandHandler<>),
				typeof(ICommandHandler<,>),
				typeof(IQueryHandler<>),
				typeof(IQueryHandler<,>)
			};

			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				foreach (var handlerInterfaceType in handlerInterfaceTypes)
				{
					RegisterGenericType(serviceCollection, type, handlerInterfaceType);
				}
			};

			serviceCollection.TryAddScoped<ICqrsProcessor, CqrsProcessor>();
	 
			return serviceCollection;
		}

		private static void RegisterGenericType(IServiceCollection serviceCollection, Type type,
			Type genericTypeDefinition)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				return;
			}

			var interfaces = type.GetInterfaces();
			var handlerInterfaceTypes = interfaces
				.Where(@interface => @interface.IsGenericType && genericTypeDefinition ==
					@interface.GetGenericTypeDefinition())
				.ToList();

			if (handlerInterfaceTypes.Count == 0)
			{
				return;
			}

			var cancellationTokenType = typeof(CancellationToken);

			foreach (var handlerInterfaceType in handlerInterfaceTypes)
			{
				var argumentType = handlerInterfaceType.GenericTypeArguments[0];
				var handlerMethod = handlerInterfaceType.GetMethod("HandleAsync",
					new[] {argumentType, cancellationTokenType});
				CqrsProcessor.Register(argumentType, (handlerInterfaceType, handlerMethod));
				var lifetime = LifetimeChecker.Get(type);
				if (lifetime.HasValue)
				{
					serviceCollection.Add(new ServiceDescriptor(handlerInterfaceType, type, lifetime.Value));
				}
				else
				{
					serviceCollection.AddScoped(handlerInterfaceType, type);
				}
			}
		}
	}
}