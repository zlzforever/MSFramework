using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Domain.Event
{
	public class DomainEventDispatcher : IDomainEventDispatcher
	{
		public static readonly Type EventHandlerBaseType;

		private readonly IServiceProvider _serviceProvider;
		private static readonly ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)> Cache;

		static DomainEventDispatcher()
		{
			EventHandlerBaseType = typeof(IDomainEventHandler<>);
			Cache = new ConcurrentDictionary<Type, (Type, MethodInfo)>();
		}

		public DomainEventDispatcher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public virtual async Task DispatchAsync(DomainEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			var eventType = @event.GetType();

			var handlerInfo = Cache.GetOrAdd(eventType, x =>
			{
				var handlerType = EventHandlerBaseType.MakeGenericType(x);
				var method = handlerType.GetMethods()[0];
				return (handlerType, method);
			});

			var handlers = _serviceProvider.GetServices(handlerInfo.Interface).Where(x => x != null);

			foreach (var handler in handlers)
			{
				if (handlerInfo.Method.Invoke(handler, new object[] {@event}) is Task task)
				{
					await task;
				}
			}
		}

		public virtual void Dispose()
		{
		}
	}
}