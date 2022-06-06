using System;

namespace MicroserviceFramework.EventBus
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class EventAttribute : Attribute
	{
		/// <summary>
		/// 事件的名称
		/// </summary>
		public string Name { get; set; }
	}
}