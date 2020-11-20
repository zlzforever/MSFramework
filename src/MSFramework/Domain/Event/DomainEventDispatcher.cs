using System;
using System.Collections.Concurrent;
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
		private static readonly Dictionary<Type, (Type Interface, MethodInfo Method)> _cache;

		static DomainEventDispatcher()
		{
			EventHandlerBaseType = typeof(IDomainEventHandler<>);
			_cache = new Dictionary<Type, (Type, MethodInfo)>();
		}

		public static void Register(Type eventType, (Type Interface, MethodInfo Method) cacheItem)
		{
			if (!_cache.ContainsKey(eventType))
			{
				_cache.Add(eventType, cacheItem);
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

			var tuple = _cache.GetOrDefault(eventType);

			if (tuple == default)
			{
				throw new MicroserviceFrameworkException("获取领域事件处理器缓存失败");
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