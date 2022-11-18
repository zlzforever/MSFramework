using System;
using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using Ordering.Application.Events;

namespace Ordering.Application.EventHandlers;

public class TestEvent1Handler : IEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent @event)
    {
        Console.WriteLine($"TestEvent1Handler: {@event.Id}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}

public class TestEvent2Handler : IEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent @event)
    {
        Console.WriteLine($"TestEvent2Handler: {@event.Id}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
