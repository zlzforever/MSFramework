using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.LocalEvent;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public class EventBusImplTests
{
    private sealed class TestHost : IAsyncDisposable
    {
        private readonly LocalEventBackgroundService _backgroundService;

        public TestHost(ServiceProvider provider, LocalEventBackgroundService backgroundService)
        {
            Provider = provider;
            _backgroundService = backgroundService;
        }

        public ServiceProvider Provider { get; }

        public LocalEventBackgroundService BackgroundService => _backgroundService;

        public async Task StopAsync()
        {
            if (_stopped)
            {
                return;
            }

            _stopped = true;
            await _backgroundService.StopAsync(CancellationToken.None);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            await Provider.DisposeAsync();
        }

        private bool _stopped;
    }

    public sealed record HostMarker(string Name);

    public record HostIsolationEvent : EventBase
    {
        public string ExpectedHost { get; init; }

        public string HandledBy { get; set; }

        public TaskCompletionSource<string> Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool BlockUntilReleased { get; init; }

        public TaskCompletionSource<bool> HandlerStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class HostIsolationEventHandler(HostMarker marker) : IEventHandler<HostIsolationEvent>
    {
        public async Task HandleAsync(HostIsolationEvent @event, CancellationToken cancellationToken)
        {
            @event.HandledBy = marker.Name;

            if (@event.BlockUntilReleased)
            {
                @event.HandlerStarted.TrySetResult(true);
                await @event.Release.Task;
            }

            @event.Processed.TrySetResult(marker.Name);
        }
    }

    public record Event1 : EventBase
    {
        public static readonly StringBuilder Output = new();
        public int Order { get; set; }

        public TaskCompletionSource<bool> Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public class SingleHandlerEventHandler : IEventHandler<Event1>
    {
        public Task HandleAsync(Event1 @event, CancellationToken cancellationToken)
        {
            Event1.Output.Append(@event.Order).Append(", ");
            @event.Processed.TrySetResult(true);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    [Fact]
    public async Task SingleSubscribeEvent()
    {
        for (var i = 0; i < 40; ++i)
        {
            Event1.Output.Clear();

            Thread.CurrentPrincipal =
                new ClaimsPrincipal(new[] { new ClaimsIdentity(new List<Claim> { new("sub", "123") }) });

            await using var host = await CreateHostAsync();
            var provider = host.Provider;

            var eventBus = provider.GetRequiredService<IEventPublisher>();

            var firstEvent = new Event1 { Order = 1 };
            await eventBus.PublishAsync(firstEvent);
            await firstEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var secondEvent = new Event1 { Order = 2 };
            await eventBus.PublishAsync(secondEvent);
            await secondEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal("1, 2, ", Event1.Output.ToString());
        }
    }

    // [EventName("event2")]
    public record Event2 : EventBase;

    // [Fact]
    // public void EventName()
    // {
    //     var name = typeof(Event1).GetEventName();
    //     Assert.Equal("MSFramework.Tests.EventBusImplTests+Event1", name);
    //
    //     var name2 = typeof(Event2).GetEventName();
    //     Assert.Equal("event2", name2);
    // }

    public record Event3 : EventBase
    {
        public static readonly StringBuilder Output = new();
        public int Order { get; set; }

        public TaskCompletionSource<bool> Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _handlerCount;

        public void MarkHandlerCompleted()
        {
            if (Interlocked.Increment(ref _handlerCount) == 2)
            {
                Processed.TrySetResult(true);
            }
        }
    }

    public class EventHandler31 : IEventHandler<Event3>
    {
        public Task HandleAsync(Event3 @event, CancellationToken cancellationToken)
        {
            lock (Event3.Output)
            {
                Event3.Output.Append(@event.Order).Append(", ");
            }

            @event.MarkHandlerCompleted();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    public class EventHandler32 : IEventHandler<Event3>
    {
        public Task HandleAsync(Event3 @event, CancellationToken cancellationToken)
        {
            lock (Event3.Output)
            {
                Event3.Output.Append(@event.Order).Append(", ");
            }

            @event.MarkHandlerCompleted();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    [Fact]
    public async Task MultiSubscribeEvent()
    {
        for (var i = 0; i < 40; ++i)
        {
            Event3.Output.Clear();
            await using var host = await CreateHostAsync(useAspNetCore: true);
            var provider = host.Provider;

            var eventBus = provider.GetRequiredService<IEventPublisher>();
            var firstEvent = new Event3 { Order = 1 };
            await eventBus.PublishAsync(firstEvent);
            await firstEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var secondEvent = new Event3 { Order = 2 };
            await eventBus.PublishAsync(secondEvent);
            await secondEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal("1, 1, 2, 2, ", Event3.Output.ToString());

            // var handler = provider.GetRequiredService<IEventHandler<Event3>>();
            // await handler.HandleAsync(new Event3 { Order = 3 });
        }
    }

    [Fact]
    public async Task HostsUseIsolatedEventChannels()
    {
        await using var firstHost = await CreateHostAsync(hostName: "first");
        await using var secondHost = await CreateHostAsync(hostName: "second");

        var firstEvent = new HostIsolationEvent { ExpectedHost = "first" };
        var secondEvent = new HostIsolationEvent { ExpectedHost = "second" };
        await firstHost.Provider.GetRequiredService<IEventPublisher>().PublishAsync(firstEvent);
        await secondHost.Provider.GetRequiredService<IEventPublisher>().PublishAsync(secondEvent);

        await Task.WhenAll(
            firstEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5)),
            secondEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.Equal(firstEvent.ExpectedHost, firstEvent.HandledBy);
        Assert.Equal(secondEvent.ExpectedHost, secondEvent.HandledBy);
    }

    [Fact]
    public async Task StoppedHostRejectsNewEventsWithoutAffectingAnotherHost()
    {
        await using var firstHost = await CreateHostAsync(hostName: "first");
        await using var secondHost = await CreateHostAsync(hostName: "second");

        var firstPublisher = firstHost.Provider.GetRequiredService<IEventPublisher>();
        var secondPublisher = secondHost.Provider.GetRequiredService<IEventPublisher>();

        var firstEvent = new HostIsolationEvent { ExpectedHost = "first" };
        await firstPublisher.PublishAsync(firstEvent);
        Assert.Equal("first", await firstEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        var blockingEvent = new HostIsolationEvent
        {
            ExpectedHost = "first",
            BlockUntilReleased = true
        };
        await firstPublisher.PublishAsync(blockingEvent);
        await blockingEvent.HandlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var queuedBeforeStop = new HostIsolationEvent { ExpectedHost = "first" };
        await firstPublisher.PublishAsync(queuedBeforeStop);

        var stopTask = firstHost.StopAsync();
        blockingEvent.Release.TrySetResult(true);
        await stopTask;

        // 既有语义：停止时已入队事件仍由内层读取循环排空，停止完成后才拒绝新发布。
        Assert.Equal("first", await queuedBeforeStop.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        var stoppedHostEvent = new HostIsolationEvent { ExpectedHost = "first" };
        await Assert.ThrowsAsync<ChannelClosedException>(() => firstPublisher.PublishAsync(stoppedHostEvent));
        Assert.False(stoppedHostEvent.Processed.Task.IsCompleted);

        var secondEvent = new HostIsolationEvent { ExpectedHost = "second" };
        await secondPublisher.PublishAsync(secondEvent);
        var handledBy = await secondEvent.Processed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(secondEvent.ExpectedHost, handledBy);
    }

    private static async Task<TestHost> CreateHostAsync(
        bool useAspNetCore = false,
        string hostName = null,
        bool startService = true)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseLocalEventPublisher();
            if (useAspNetCore)
            {
                x.UseAspNetCoreExtension();
            }
        });
        if (useAspNetCore)
        {
            serviceCollection.AddHttpContextAccessor();
        }

        if (hostName != null)
        {
            serviceCollection.AddSingleton(new HostMarker(hostName));
        }

        serviceCollection.AddSingleton<LocalEventBackgroundService>();
        var provider = serviceCollection.BuildServiceProvider();
        provider.UseMicroserviceFramework();

        var backgroundService = provider.GetRequiredService<LocalEventBackgroundService>();
        if (startService)
        {
            await backgroundService.StartAsync(CancellationToken.None);
        }

        return new TestHost(provider, backgroundService);
    }
}
