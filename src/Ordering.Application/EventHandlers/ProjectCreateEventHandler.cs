using System;
using System.Threading.Tasks;
using MicroserviceFramework.Domain.Events;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.EventHandlers
{
	public class ProjectCreateEventHandler : IEventHandler<ProjectCreateEvent>
	{
		public Task HandleAsync(ProjectCreateEvent @event)
		{
			Console.WriteLine("hi");
			return Task.CompletedTask;
		}
	}
}