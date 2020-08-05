using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Application
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRequestProcessor(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddRequestProcessor(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddRequestProcessor(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();
			var cache = new RequestHandlerTypeCache();

			var handlerInterfaceTypes = new[] {typeof(IRequestHandler<>), typeof(IRequestHandler<,>)};
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
					throw new MSFrameworkException($"{type.FullName} should impl one handler");
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
					throw new MSFrameworkException("Can't find handler method");
				}

				if (cache.TryAdd(requestType, (type, method)))
				{
					serviceCollection.TryAddScoped(type);
				}
				else
				{
					throw new MSFrameworkException(
						$"Register {requestType.FullName} with handler {type.FullName} failed");
				}
			}

			serviceCollection.TryAddSingleton(cache);
			serviceCollection.TryAddScoped<IRequestProcessor, DefaultRequestProcessor>();

			return serviceCollection;
		}
	}
}