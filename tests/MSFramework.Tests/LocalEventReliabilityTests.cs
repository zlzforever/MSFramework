using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.LocalEvent;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public sealed class LocalEventReliabilityTests
{
    public sealed record AlwaysFailsEvent : EventBase
    {
        public int Attempts;
    }

    public sealed class AlwaysFailsHandler : IEventHandler<AlwaysFailsEvent>
    {
        public Task HandleAsync(AlwaysFailsEvent @event, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref @event.Attempts);
            throw new InvalidOperationException("expected failure");
        }
    }

    public sealed record DrainEvent : EventBase
    {
        public static int Handled;
    }

    public sealed class DrainHandler : IEventHandler<DrainEvent>
    {
        public async Task HandleAsync(DrainEvent @event, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            Interlocked.Increment(ref DrainEvent.Handled);
        }
    }

    public sealed record CriticalEvent : EventBase, ICriticalLocalEvent;

    [Fact]
    public async Task FailedHandler_RetriesThenWritesObservableDeadLetter()
    {
        await using var host = CreateHost();
        await host.Background.StartAsync(CancellationToken.None);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();
        var @event = new AlwaysFailsEvent();

        await publisher.PublishAsync(@event);
        await WaitUntilAsync(() => host.DeadLetters.Entries.Count == 1, TimeSpan.FromSeconds(5));

        Assert.Equal(3, @event.Attempts);
        var deadLetter = Assert.Single(host.DeadLetters.Entries);
        Assert.Same(@event, deadLetter.EventData);
        Assert.Equal(3, deadLetter.Attempts);
        Assert.Equal(typeof(AlwaysFailsHandler), deadLetter.HandlerType);
    }

    [Fact]
    public async Task StopAsync_DrainsEventsAcceptedBeforeShutdown()
    {
        DrainEvent.Handled = 0;
        await using var host = CreateHost();
        await host.Background.StartAsync(CancellationToken.None);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();
        for (var i = 0; i < 5; i++)
        {
            await publisher.PublishAsync(new DrainEvent());
        }

        await host.Background.StopAsync(CancellationToken.None);

        Assert.Equal(5, DrainEvent.Handled);
    }

    [Fact]
    public async Task CriticalEvent_IsRejectedByNonDurableLocalPublisher()
    {
        await using var host = CreateHost();
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => publisher.PublishAsync(new CriticalEvent()));

        Assert.Contains("outbox", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static TestHost CreateHost()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher(options =>
        {
            options.MaxDeliveryAttempts = 3;
            options.RetryDelay = TimeSpan.FromMilliseconds(1);
            options.ShutdownDrainTimeout = TimeSpan.FromSeconds(5);
        }));
        var provider = services.BuildServiceProvider();
        provider.UseMicroserviceFramework();
        return new TestHost(
            provider,
            provider.GetRequiredService<LocalEventBackgroundService>(),
            (InMemoryLocalEventDeadLetterStore)provider.GetRequiredService<ILocalEventDeadLetterStore>());
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException();
            }

            await Task.Delay(10);
        }
    }

    private sealed class TestHost(
        ServiceProvider provider,
        LocalEventBackgroundService background,
        InMemoryLocalEventDeadLetterStore deadLetters) : IAsyncDisposable
    {
        public ServiceProvider Provider { get; } = provider;
        public LocalEventBackgroundService Background { get; } = background;
        public InMemoryLocalEventDeadLetterStore DeadLetters { get; } = deadLetters;

        public async ValueTask DisposeAsync()
        {
            await Background.StopAsync(CancellationToken.None);
            await Provider.DisposeAsync();
        }
    }
}
