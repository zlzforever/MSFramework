using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IDomainEvent : IEvent
	{
	}

	public class LocalDomainEvent : Event, IDomainEvent
	{
	}

	public class DistributedDomainEvent : Event, IDomainEvent
	{
	}
}