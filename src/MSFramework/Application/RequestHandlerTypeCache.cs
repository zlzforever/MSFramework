using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MSFramework.Application
{
	public class RequestHandlerTypeCache
	{
		private readonly ConcurrentDictionary<Type, (Type, MethodInfo)> _cache =
			new ConcurrentDictionary<Type, (Type, MethodInfo)>();

		public void TryAdd(Type commandType, (Type, MethodInfo) tuple)
		{
			_cache.TryAdd(commandType, tuple);
		}

		public bool ContainsKey(Type commandType)
		{
			return _cache.ContainsKey(commandType);
		}

		public bool TryGetValue(Type commandType, out (Type, MethodInfo) handlerType)
		{
			var result = _cache.TryGetValue(commandType, out handlerType);
			return result;
		}
	}
}