using System;

namespace MSFramework.Domain
{
	public static class AggregateRootFactory
	{
		public static T CreateAggregate<T>()
		{
			return (T)Activator.CreateInstance(typeof(T), true);
		}
	}
}