using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class EventBusTests
	{
		public class Event1 : EventBase
		{
			public int Order { get; set; }

			public Event1(int i)
			{
				Order = i;
			}
		}

		public class Event1Handler : IEventHandler<Event1>
		{
			public static ConcurrentBag<int> Result = new ConcurrentBag<int>();

			public Task HandleAsync(Event1 @event)
			{
				Result.Add(@event.Order);
				return Task.CompletedTask;
			}

			public void Dispose()
			{
				
			}
		}

		[Fact]
		public async Task PubSub()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddEventBus();
			serviceCollection.AddMicroserviceFramework();

			var provider = serviceCollection.BuildServiceProvider();
			var eventBus = provider.GetRequiredService<IEventBus>();

			for (var i = 0; i < 100; ++i)
			{
				await eventBus.PublishAsync(new Event1(1));
			}

			Thread.Sleep(1000);
		}
	}
}