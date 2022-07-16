using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.Domain
{
    public interface IDomainEventHandler<in TMessage> : IRequestHandler<TMessage> where TMessage : DomainEvent
    {
    }
}