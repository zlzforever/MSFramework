using System;
using MicroserviceFramework.EventBus;

namespace Ordering.Application.Events;

/// <summary>
/// 发送到外部的领域事件
/// </summary>
public record OrderStartedEvent(string userId, Guid orderId) : EventBase
{
    public string UserId { get; } = userId;

    public Guid OrderId { get; } = orderId;
}
