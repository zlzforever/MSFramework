using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.EventSource;
using MSFramework.Security;
using Ordering.API.Application.Dto.Order;
using Ordering.API.Application.Event;
using Ordering.Domain.AggregateRoot.Order;
using Ordering.Domain.Repository;

namespace Ordering.API.Application.Command
{
	public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
	{
		private readonly IOrderingRepository _orderRepository;
		private readonly IEventSourceService _eventSourceService;
		private readonly ILogger<CreateOrderCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		// Using DI to inject infrastructure persistence Repositories
		public CreateOrderCommandHandler(IMediator mediator,
			IEventSourceService eventSourceService,
			IOrderingRepository orderRepository,
			IUnitOfWork unitOfWork,
			ILogger<CreateOrderCommandHandler> logger)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_eventSourceService = eventSourceService ??
			                                   throw new ArgumentNullException(nameof(eventSourceService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
		{
			// Add Integration event to clean the basket
			var orderStartedIntegrationEvent = new OrderStartedEvent(message.UserId);
			await _eventSourceService.AddEventAsync(orderStartedIntegrationEvent);

			// Add/Update the Buyer AggregateRoot
			// DDD patterns comment: Add child entities and value-objects through the Order Aggregate-Root
			// methods and constructor so validations, invariants and business logic 
			// make sure that consistency is preserved across the whole aggregate
			var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);
			var order = new Order(message.UserId, message.UserName, address, message.CardTypeId, message.CardNumber,
				message.CardSecurityNumber, message.CardHolderName, message.CardExpiration);

 
			
			foreach (var item in message.OrderItems)
			{
				order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl,
					item.Units);
			}

			_logger.LogInformation("----- Creating Order - Order: {@Order}", order);

			await _orderRepository.InsertAsync(order);

			await _unitOfWork.CommitAsync();
			return true;
		}
	}
}