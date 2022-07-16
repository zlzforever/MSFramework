using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MicroserviceFramework.Mediator
{
    internal class MediatorTypeMapper : IMediatorTypeMapper
    {
        private readonly ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)> _cache;

        public MediatorTypeMapper()
        {
            _cache = new ConcurrentDictionary<Type, (Type Interface, MethodInfo Method)>();
        }

        public (Type HandlerType, MethodInfo MethodInfo) Get(Type type,
            Func<Type, (Type HandlerType, MethodInfo MethodInfo)> valueFactory)
        {
            return _cache.GetOrAdd(type, valueFactory);
        }
    }
}