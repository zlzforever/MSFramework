using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IDomainEvent : IEvent
	{
	}

	public abstract class LocalDomainEvent : Event, IDomainEvent
	{
	}

	public abstract class DistributedDomainEvent : Event, IDomainEvent
	{
	}
}