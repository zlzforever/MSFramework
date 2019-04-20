using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MediatR;
using MSFramework.EventBus;
using MSFramework.EventSource;
using Ordering.API.Application.Event;
using Ordering.Domain.AggregateRoot.Order;
using Ordering.Domain.Events;
using Ordering.Domain.Repository;

namespace Ordering.API.Application.DomainEventHandler
{
	public class OrderShippedDomainEventHandler
		: IEventHandler<OrderShippedDomainEvent>
	{
		private readonly IOrderingRepository _orderRepository;
		private readonly IBuyerRepository _buyerRepository;
		private readonly IEventSourceService _eventSourceService;
		private readonly ILoggerFactory _logger;

		public OrderShippedDomainEventHandler(
			IOrderingRepository orderRepository,
			ILoggerFactory logger,
			IBuyerRepository buyerRepository,
			IEventSourceService eventSourceService)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
			_eventSourceService = eventSourceService;
		}


		public async Task Handle(OrderShippedDomainEvent @event)
		{
//			_logger.CreateLogger<OrderShippedDomainEvent>()
//				.LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
//					orderShippedDomainEvent.Order.Id, nameof(OrderStatus.Shipped), OrderStatus.Shipped.Id);

			var order = await _orderRepository.GetAsync(@event.Order.Id);
			var buyer = await _buyerRepository.GetAsync(order.BuyerId.Value);

			var orderStatusChangedToShippedIntegrationEvent =
				new OrderStatusChangedToShippedIntegrationEvent(order.Id, order.OrderStatus.Name, buyer.Name);
			await _eventSourceService.AddEventAsync(orderStatusChangedToShippedIntegrationEvent);
		}
	}
}