using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public class EventBusTests
{
    public class Event1 : EventBase
    {
        public static readonly StringBuilder Output = new();
        public int Order { get; set; }
    }

    public class SingleHandlerEventHandler : IEventHandler<Event1>
    {
        private readonly ISession _session;

        public SingleHandlerEventHandler(ISession session)
        {
            _session = session;
        }

        public Task HandleAsync(Event1 @event)
        {
            Console.WriteLine(_session.UserId);
            Event1.Output.Append(@event.Order).Append(", ");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    [Fact]
    public void SingleSubscribeEvent()
    {
        for (var i = 0; i < 40; ++i)
        {
            Event1.Output.Clear();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMicroserviceFramework(x =>
            {
                x.UseDependencyInjectionLoader();
                x.UseAspNetCore();
                x.UseEventBus();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseMicroserviceFramework();

            var eventBus = provider.GetRequiredService<IEventPublisher>();

            eventBus.Publish(new Event1 { Order = 1 });
            Thread.Sleep(25);
            eventBus.Publish(new Event1 { Order = 2 });
            Thread.Sleep(25);
            Assert.Equal("1, 2, ", Event1.Output.ToString());
        }
    }

    [EventName("event2")]
    public class Event2 : EventBase
    {
    }

    [Fact]
    public void EventName()
    {
        var name = typeof(Event1).GetEventName();
        Assert.Equal("MSFramework.Tests.EventBusTests+Event1", name);

        var name2 = typeof(Event2).GetEventName();
        Assert.Equal("event2", name2);
    }

    public class Event3 : EventBase
    {
        public static readonly StringBuilder Output = new();
        public int Order { get; set; }
    }

    public class EventHandler31 : IEventHandler<Event3>
    {
        public Task HandleAsync(Event3 @event)
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
        public Task HandleAsync(Event3 @event)
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
    public void MultiSubscribeEvent()
    {
        for (var i = 0; i < 40; ++i)
        {
            Event3.Output.Clear();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMicroserviceFramework(x =>
            {
                x.UseDependencyInjectionLoader();
                x.UseEventBus();
                x.UseAspNetCore();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseMicroserviceFramework();

            var eventBus = provider.GetRequiredService<IEventPublisher>();

            eventBus.Publish(new Event3 { Order = 1 });
            Thread.Sleep(20);
            eventBus.Publish(new Event3 { Order = 2 });
            Thread.Sleep(20);
            Assert.Equal("1, 1, 2, 2, ", Event3.Output.ToString());
        }
    }
}
