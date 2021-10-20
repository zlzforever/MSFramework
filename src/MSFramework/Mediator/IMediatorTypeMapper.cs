using System;
using System.Reflection;

namespace MicroserviceFramework.Mediator
{
	public interface IMediatorTypeMapper
	{
		(Type HandlerType, MethodInfo MethodInfo) Get(Type type,
			Func<Type, (Type HandlerType, MethodInfo MethodInfo)> valueFactory);
	}
}