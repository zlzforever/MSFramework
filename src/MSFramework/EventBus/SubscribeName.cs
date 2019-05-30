using System;

namespace MSFramework.EventBus
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SubscribeName : Attribute
	{
		public string Name { get; private set; }

		public SubscribeName(string name)
		{
			Name = name;
		}
	}
}