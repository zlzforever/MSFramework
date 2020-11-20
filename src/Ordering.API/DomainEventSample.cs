using System;
using System.Threading.Tasks;
using MicroserviceFramework.Domain.Event;

namespace Ordering.API
{
	public class DomainEventSample1 : DomainEvent
	{
	}

	public class DomainEventSample2 : DomainEvent
	{
	}

	public class DomainEventSampleHandler1 : IDomainEventHandler<DomainEventSample1>
		, IDomainEventHandler<DomainEventSample2>
	{
		public Task HandleAsync(DomainEventSample1 @event)
		{
			Console.WriteLine("Handle sample event 1 by handler 1");
			return Task.CompletedTask;
		}

		public Task HandleAsync(DomainEventSample2 @event)
		{
			Console.WriteLine("Handle sample event 2 by handler 1");
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}

	public class DomainEventSampleHandler2 : IDomainEventHandler<DomainEventSample1>
	{
		public Task HandleAsync(DomainEventSample1 @event)
		{
			Console.WriteLine("Handle sample event 1 by handler 2");
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}
}