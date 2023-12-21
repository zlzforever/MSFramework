using System;
using MicroserviceFramework.EventBus;

namespace Ordering.Application.Events;

public record TestEvent : EventBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
