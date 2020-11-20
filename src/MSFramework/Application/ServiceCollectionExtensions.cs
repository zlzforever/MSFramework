using System.Linq;
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
		public static IServiceCollection AddCQRS(this IServiceCollection serviceCollection)
		{
			var cache = new HandlerTypeCache();
			var handlerInterfaceTypes = new[]
			{
				typeof(ICommandHandler<>),
				typeof(ICommandHandler<,>),
				typeof(IQueryHandler<>),
				typeof(IQueryHandler<,>)
			};

			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				var handlerTypes = type.GetInterfaces()
					.Where(@interface => @interface.IsGenericType &&
					                     handlerInterfaceTypes.Any(x => x == @interface.GetGenericTypeDefinition()))
					.ToList();

				if (handlerTypes.Count == 0)
				{
					return;
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
					var lifetime = LifetimeChecker.Get(type);
					if (lifetime.HasValue)
					{
						serviceCollection.Add(new ServiceDescriptor(type, lifetime.Value));
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
			};

			serviceCollection.TryAddSingleton(cache);
			serviceCollection.TryAddScoped<ICommandProcessor, DefaultCommandProcessor>();
			serviceCollection.TryAddScoped<IQueryProcessor, DefaultQueryProcessor>();
			return serviceCollection;
		}
	}
}