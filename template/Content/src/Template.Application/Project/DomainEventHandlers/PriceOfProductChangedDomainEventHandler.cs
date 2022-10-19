using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Application.Project.DomainEventHandlers
{
	public class PriceOfProductChangedDomainEventHandler : IDomainEventHandler<PriceOfProductChangedEvent>
	{
		public void Dispose()
		{
		}

		public Task HandleAsync(PriceOfProductChangedEvent query, CancellationToken cancellationToken = new())
		{
			// todo: do something or send event to other app
			return Task.CompletedTask;
		}
	}
}