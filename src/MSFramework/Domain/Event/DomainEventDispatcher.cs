using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Domain.Event
{
	public class DomainEventDispatcher : IDomainEventDispatcher
	{
		public static readonly Type EventHandlerBaseType;

		private readonly IServiceProvider _serviceProvider;
		private static readonly Dictionary<Type, (Type Interface, MethodInfo Method)> Cache;

		static DomainEventDispatcher()
		{
			EventHandlerBaseType = typeof(IDomainEventHandler<>);
			Cache = new Dictionary<Type, (Type, MethodInfo)>();
		}

		public static void Register(Type eventType, (Type Interface, MethodInfo Method) cacheItem)
		{
			lock (Cache)
			{
				if (!Cache.ContainsKey(eventType))
				{
					Cache.Add(eventType, cacheItem);
				}
			}
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

			var tuple = Cache.GetOrDefault(eventType);

			if (tuple == default)
			{
				return;
			}

			var handlers = _serviceProvider.GetServices(tuple.Interface);

			foreach (var handler in handlers)
			{
				if (tuple.Method.Invoke(handler, new object[] {@event}) is Task task)
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