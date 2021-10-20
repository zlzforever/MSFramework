using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class Event4 : DomainEvent
	{
		public static int Count;
	}

	public class Event1Handler : IDomainEventHandler<Event4>
	{
		public Task HandleAsync(Event4 @event)
		{
			Event4.Count += 1;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}

	public class Event2 : DomainEvent
	{
	}

	public class Event3 : DomainEvent
	{
		public static int Count;
	}

	public class Event31Handler : IDomainEventHandler<Event3>
	{
		public Task HandleAsync(Event3 @event)
		{
			Event3.Count += 1;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}

	public class Event32Handler : IDomainEventHandler<Event3>
	{
		public Task HandleAsync(Event3 @event)
		{
			Event3.Count += 1;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}

	public class DomainEventTests
	{
		[Fact]
		public async Task DispatchTo1HandlerTests()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(
				x => { });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			
			var mediator = serviceProvider.GetRequiredService<IMediator>();
			await mediator.PublishAsync(new Event4());
			Assert.Equal(1, Event4.Count);

			await mediator.PublishAsync(new Event4());
			Assert.Equal(2, Event4.Count);
		}

		[Fact]
		public async Task DispatchToMultiHandlerTests()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(
				x => { });

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var mediator = serviceProvider.GetRequiredService<IMediator>();
			await mediator.PublishAsync(new Event3());
			Assert.Equal(2, Event3.Count);

			await mediator.PublishAsync(new Event3());
			Assert.Equal(4, Event3.Count);
		}

		[Fact]
		public async Task EventWithoutHandlerTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(
				x => { });

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var mediator = serviceProvider.GetRequiredService<IMediator>();
			await mediator.PublishAsync(new Event2());
		}
	}
}