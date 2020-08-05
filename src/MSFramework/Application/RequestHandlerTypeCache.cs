using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MSFramework.Application
{
	public class RequestHandlerTypeCache
	{
		private readonly ConcurrentDictionary<Type, (Type, MethodInfo)> _cache =
			new ConcurrentDictionary<Type, (Type, MethodInfo)>();

		public bool TryAdd(Type requestType, (Type, MethodInfo) tuple)
		{
			return _cache.TryAdd(requestType, tuple);
		}

		public bool TryGetValue(Type requestType, out (Type, MethodInfo) handlerType)
		{
			var result = _cache.TryGetValue(requestType, out handlerType);
			return result;
		}
	}
}