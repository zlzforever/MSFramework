using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Application.Project.DomainEventHandlers
{
	public class PriceOfProductChangedDomainEventHandler : IDomainEventHandler<PriceOfProductChangedEvent>
	{
		public Task HandleAsync(PriceOfProductChangedEvent @event)
		{
			// todo: do something or send event to other app
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}
	}
}