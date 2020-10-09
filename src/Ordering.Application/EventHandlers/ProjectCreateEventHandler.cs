using System;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.EventBus;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.EventHandlers
{
	public class ProjectCreatedIntegrationEvent : IntegrationEvent
	{
	}

	public class ProjectCreatedIntegrationEventHandler : IIntegrationEventHandler<ProjectCreatedIntegrationEvent>,
		IScopeDependency
	{
		public Task HandleAsync(ProjectCreatedIntegrationEvent @event)
		{
			Console.WriteLine("Execute ProjectCreatedIntegrationEvent");
			return Task.CompletedTask;
		}
	}

	public class ProjectCreateEventHandler : IEventHandler<ProjectCreateEvent>
	{
		private readonly IEventBus _eventBus;

		public ProjectCreateEventHandler(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public async Task HandleAsync(ProjectCreateEvent @event)
		{
			Console.WriteLine("Execute ProjectCreateEvent");
			await _eventBus.PublishAsync(new ProjectCreatedIntegrationEvent());
		}
	}
}