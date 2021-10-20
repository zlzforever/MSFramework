using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.Domain
{
	public interface IDomainEventHandler<in TMessage> : IMessageHandler<TMessage> where TMessage : DomainEvent
	{
	}
}