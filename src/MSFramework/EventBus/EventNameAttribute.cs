using System;
using MicroserviceFramework.Utilities;

namespace MicroserviceFramework.EventBus
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class EventNameAttribute : Attribute
	{
		/// <summary>
		/// 事件的名称
		/// </summary>
		public string Name { get; }

		public EventNameAttribute(string name)
		{
			Check.NotNullOrWhiteSpace(name, nameof(name));
			Name = name;
		}
	}
}