using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain.Event;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class Event1 : DomainEvent
	{
		public static int Count;
	}

	public class Event2 : DomainEvent
	{
	}

	public class Event1Handler : IDomainEventHandler<Event1>
	{
		public Task HandleAsync(Event1 @event)
		{
			Event1.Count += 1;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}

	public class DomainEventTests
	{
		[Fact]
		public async Task DispatchTests()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(
				x => { });

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var eventDispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();
			await eventDispatcher.DispatchAsync(new Event1());
			Assert.Equal(1, Event1.Count);

			await eventDispatcher.DispatchAsync(new Event1());
			Assert.Equal(2, Event1.Count);
		}

		[Fact]
		public async Task EventWithoutHandlerTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(
				x => { });

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var eventDispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();
			await eventDispatcher.DispatchAsync(new Event2());
		}
	}
}