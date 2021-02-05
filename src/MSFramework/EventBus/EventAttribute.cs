using System;

namespace MicroserviceFramework.EventBus
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class EventAttribute : Attribute
	{
	}
}