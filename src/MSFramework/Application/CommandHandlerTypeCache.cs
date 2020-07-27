using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MSFramework.Application
{
	public class CommandHandlerTypeCache
	{
		private readonly ConcurrentDictionary<Type, (Type, MethodInfo)> _cache =
			new ConcurrentDictionary<Type, (Type, MethodInfo)>();

		public void TryAdd(Type commandType, Type handlerType)
		{
			_cache.TryAdd(commandType, (handlerType, handlerType.GetMethod("HandleAsync")));
		}

		public bool Contains(Type commandType)
		{
			return _cache.ContainsKey(commandType);
		}

		public bool TryGetHandlerType(Type commandType, out (Type, MethodInfo) handlerType)
		{
			var result = _cache.TryGetValue(commandType, out handlerType);
			return result;
		}
	}
}