using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.EventBus.RabbitMQ;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
    public class EventBusTests
    {
        public class Event1 : EventBase
        {
            public int Order { get; set; }
        }

        public class Event1Handler : IEventHandler<Event1>
        {
            public static int Count;

            public Task HandleAsync(Event1 @event)
            {
                Interlocked.Increment(ref Count);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }

        [EventName("event2")]
        public class Event2 : EventBase
        {
        }

        public class Event2Handler : IEventHandler<Event2>
        {
            public static int Count;

            public Task HandleAsync(Event2 @event)
            {
                Interlocked.Increment(ref Count);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }

        [Fact]
        public void EventName()
        {
            var name = typeof(Event1).GetEventName();
            Assert.Equal("MSFramework.Tests.EventBusTests+Event1", name);

            var name2 = typeof(Event2).GetEventName();
            Assert.Equal("event2", name2);
        }

        [Fact]
        public async Task InProcessEventBus()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMicroserviceFramework(x =>
            {
                x.UseDependencyInjectionLoader();
                x.UseEventBus();
            });

            var provider = serviceCollection.BuildServiceProvider();
            var eventBus = provider.GetRequiredService<IEventBus>();

            for (var i = 0; i < 100; ++i)
            {
                await eventBus.PublishAsync(new Event1
                {
                    Order = 1
                });
            }

            Thread.Sleep(1000);
            Assert.Equal(100, Event1Handler.Count);
        }

        [Fact]
        public async Task RabbitMQEventBus()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMicroserviceFramework(x =>
            {
                x.UseDependencyInjectionLoader();
                x.UseEventBusRabbitMQ(configuration);
            });

            var provider = serviceCollection.BuildServiceProvider();
            var eventBus = provider.GetRequiredService<IEventBus>();

            for (var i = 0; i < 100; ++i)
            {
                await eventBus.PublishAsync(new Event2());
            }

            Thread.Sleep(2000);
            Assert.Equal(100, Event2Handler.Count);
        }
    }
}