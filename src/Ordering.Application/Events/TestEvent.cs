using System;
using MicroserviceFramework.EventBus;

namespace Ordering.Application.Events;

public class TestEvent : EventBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
