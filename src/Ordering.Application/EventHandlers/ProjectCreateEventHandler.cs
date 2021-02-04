using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.EventBus;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Application.EventHandlers
{
	public class ProjectCreatedIntegrationEvent : EventBase
	{
	}

	public class ProjectCreatedIntegrationEventHandler : IEventHandler<ProjectCreatedIntegrationEvent>,
		IScopeDependency
	{
		private readonly IProductRepository _productRepository;
		private readonly UnitOfWorkManager _uowManager;

		public ProjectCreatedIntegrationEventHandler(IProductRepository productRepository, UnitOfWorkManager uowManager)
		{
			_productRepository = productRepository;
			_uowManager = uowManager;
		}

		public async Task HandleAsync(ProjectCreatedIntegrationEvent @event)
		{
			var result = await _productRepository.PagedQueryAsync(1, 10);
			var product = result.Data.Last();
			product.SetName(Guid.NewGuid().ToString());
			await _uowManager.CommitAsync();
		}

		public void Dispose()
		{
		}
	}

	public class ProjectCreateEventHandler : IDomainEventHandler<ProjectCreateEvent>
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
		
		public void Dispose()
		{
		}
	}
}