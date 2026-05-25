using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.LocalEvent;
using Ordering.Application.Events;

namespace Ordering.Application.EventHandlers;

public class TestEvent1Handler : IEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
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
    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
    {
        Console.WriteLine($"TestEvent2Handler: {@event.Id}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
