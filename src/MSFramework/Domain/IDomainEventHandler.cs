using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 领域事件处理器
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>
/// 同 <see cref="IRequestHandler{TRequest}"/>：Mediator 按"精确事件类型"闭泛型查找，
/// 不存在变体匹配，故不做逆变（in）标注——为领域事件基类注册的 Handler 不会被派发命中
/// </remarks>
public interface IDomainEventHandler<TMessage> : IRequestHandler<TMessage> where TMessage : DomainEvent;
