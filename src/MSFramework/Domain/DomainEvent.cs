using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 领域事件
/// 领域事件通过中介者发送到应用层，由应用层的处理器进行处理
/// 必须是同一事务
/// </summary>
public abstract class DomainEvent : IRequest
{
}
