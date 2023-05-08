using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 领域事件处理器接口
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IDomainEventHandler<in TMessage> : IRequestHandler<TMessage> where TMessage : DomainEvent
{
}
