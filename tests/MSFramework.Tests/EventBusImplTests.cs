using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
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

        public async ValueTask DisposeAsync()
        {
            await _backgroundService.StopAsync(CancellationToken.None);
            await Provider.DisposeAsync();
        }
    }

    public sealed record HostMarker(string Name);

    public record HostIsolationEvent : EventBase
    {
        public string ExpectedHost { get; init; }

        public string HandledBy { get; set; }

        public TaskCompletionSource<string> Processed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class HostIsolationEventHandler(HostMarker marker) : IEventHandler<HostIsolationEvent>
    {
        public Task HandleAsync(HostIsolationEvent @event, CancellationToken cancellationToken)
        {
            @event.HandledBy = marker.Name;
            @event.Processed.TrySetResult(marker.Name);
            return Task.CompletedTask;
        }
    }

    public record Event1 : EventBase
    {
        public static readonly StringBuilder Output = new();
        public int Order { get; set; }
    }

    public class SingleHandlerEventHandler : IEventHandler<Event1>
    {
        public Task HandleAsync(Event1 @event, CancellationToken cancellationToken)
        {
            Event1.Output.Append(@event.Order).Append(", ");
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

            await eventBus.PublishAsync(new Event1 { Order = 1 });
            Thread.Sleep(100);
            await eventBus.PublishAsync(new Event1 { Order = 2 });
            Thread.Sleep(100);
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
    }

    public class EventHandler31 : IEventHandler<Event3>
    {
        public Task HandleAsync(Event3 @event, CancellationToken cancellationToken)
        {
            lock (Event3.Output)
            {
                Event3.Output.Append(@event.Order).Append(", ");
            }

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
            await eventBus.PublishAsync(new Event3 { Order = 1 });
            Thread.Sleep(100);
            await eventBus.PublishAsync(new Event3 { Order = 2 });
            Thread.Sleep(100);
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
    public async Task EventPublishedToStoppedHostIsNotConsumedByAnotherHost()
    {
        await using var firstHost = await CreateHostAsync(hostName: "first");
        await using var secondHost = await CreateHostAsync(hostName: "second", startService: false);

        var secondEvent = new HostIsolationEvent { ExpectedHost = "second" };
        await secondHost.Provider.GetRequiredService<IEventPublisher>().PublishAsync(secondEvent);
        await Task.Delay(200);

        Assert.False(secondEvent.Processed.Task.IsCompleted);

        await secondHost.BackgroundService.StartAsync(CancellationToken.None);
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
            await Task.Delay(100);
        }

        return new TestHost(provider, backgroundService);
    }
}
