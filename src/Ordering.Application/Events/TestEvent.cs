using System;
using MicroserviceFramework.LocalEvent;

namespace Ordering.Application.Events;

public record TestEvent : EventBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
