using System.Collections.Concurrent;
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
			public static ConcurrentBag<int> Result = new();

			public Task HandleAsync(Event1 @event)
			{
				Result.Add(@event.Order);
				return Task.CompletedTask;
			}

			public void Dispose()
			{
			}
		}

		[Event]
		public class Event2
		{
			/// <summary>
			/// 事件源标识
			/// </summary>
			public string EventId { get; }

			/// <summary>
			/// 事件发生时间
			/// </summary>
			public long EventTime { get; }
		}

		public class Event2Handler : IEventHandler<Event2>
		{
			private static readonly object Locker = new object();
			public static int Count;

			public Task HandleAsync(Event2 @event)
			{
				lock (Locker)
				{
					Count += 1;
				}

				return Task.CompletedTask;
			}

			public void Dispose()
			{
			}
		}

		[Fact]
		public async Task PubDynamicEvent()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddMicroserviceFramework(x => { x.UseEventBus(); });

			var provider = serviceCollection.BuildServiceProvider();
			var eventBus = provider.GetRequiredService<IEventBus>();

			await eventBus.PublishAsync(new Event2());

			Assert.Equal(1, Event2Handler.Count);
			Thread.Sleep(1000);
		}

		[Fact]
		public async Task PubSub()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddMicroserviceFramework(x => { x.UseEventBus(); });

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