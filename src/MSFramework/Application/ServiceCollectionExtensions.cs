using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.Application.CQRS.Command;
using MicroserviceFramework.Application.CQRS.Query;
using MicroserviceFramework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Application
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddCQRS(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddCQRS(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddCQRS(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();
			var cache = new HandlerTypeCache();

			var handlerInterfaceTypes = new[]
			{
				typeof(ICommandHandler<>),
				typeof(ICommandHandler<,>),
				typeof(IQueryHandler<>),
				typeof(IQueryHandler<,>)
			};

			var singletonType = typeof(ISingletonDependency);
			var transientType = typeof(ITransientDependency);
			var scopeType = typeof(IScopeDependency);

			foreach (var type in types)
			{
				var interfaces = type.GetInterfaces();
				var handlerTypes = interfaces
					.Where(@interface => @interface.IsGenericType)
					.Where(@interface => handlerInterfaceTypes.Any(x => x == @interface.GetGenericTypeDefinition()))
					.ToList();

				if (handlerTypes.Count == 0)
				{
					continue;
				}

				if (handlerTypes.Count > 1)
				{
					throw new MicroserviceFrameworkException($"{type.FullName} should impl one handler");
				}

				var handlerType = handlerTypes.First();
				var requestType = handlerType.GenericTypeArguments.First();

				var method = type.GetMethods()
					.FirstOrDefault(x =>
						x.Name == "HandleAsync"
						&& x.GetParameters().Length == 2
						&& x.GetParameters()[0].ParameterType == requestType
						&& x.GetParameters()[1].ParameterType == typeof(CancellationToken));
				if (method == null)
				{
					throw new MicroserviceFrameworkException("找不到处理方法");
				}

				if (cache.TryAdd(requestType, (type, method)))
				{
					if (transientType.IsAssignableFrom(type))
					{
						serviceCollection.TryAddTransient(type);
					}
					else if (scopeType.IsAssignableFrom(type))
					{
						serviceCollection.TryAddScoped(type);
					}
					else if (singletonType.IsAssignableFrom(type))
					{
						serviceCollection.TryAddSingleton(type);
					}
					else
					{
						serviceCollection.TryAddScoped(type);
					}
				}
				else
				{
					throw new MicroserviceFrameworkException(
						$"Register {requestType.FullName} with handler {type.FullName} failed");
				}
			}

			serviceCollection.TryAddSingleton(cache);
			serviceCollection.TryAddScoped<ICommandProcessor, DefaultCommandProcessor>();

			return serviceCollection;
		}
	}
}