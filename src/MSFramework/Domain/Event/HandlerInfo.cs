using System;
using System.Reflection;

namespace MicroserviceFramework.Domain.Event
{
	public class HandlerInfo
	{
		private readonly Type _type;

		public Type Type => _type;
		public MethodInfo Method { get; private set; }

		public HandlerInfo(Type type, MethodInfo method)
		{
			_type = type;
			Method = method;
		}

		public override int GetHashCode()
		{
			return _type.GetHashCode();
		}
	}
}