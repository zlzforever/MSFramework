using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.EventBus;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Event;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.API.Application.Services
{
	public class OrderingAppService : ApplicationServiceBase, IOrderingAppService
	{
		private readonly IOrderRepository _repository;
		private readonly IEventBus _eventBus;

		public OrderingAppService(IMSFrameworkSession session, IEventBus eventBus,
			IOrderRepository readRepository,
			ILogger<OrderingAppService> logger) : base(session, logger)
		{
			_repository = readRepository;
			_eventBus = eventBus;
		}

		public async Task DeleteOrder(DeleteOrderDto dto)
		{
			var item = await _repository.GetAsync(dto.OrderId);
			item.Delete();
			Logger.LogInformation($"DELETED ORDER: {dto.OrderId}");
		}

		public async Task ChangeOrderAddress(ChangeOrderAddressDTO dto)
		{
			var item = await _repository.GetAsync(dto.OrderId);
			item.ChangeAddress(dto.NewAddress);
		}

		public async Task CreateOrder(CreateOrderDTO dto)
		{
			var order = new Order(
				dto.UserId,
				new Address(dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode),
				dto.Description,
				dto.OrderItems.Select(x => x.ToOrderItem()).ToList());
			await _eventBus.PublishAsync(new OrderStartedEvent(Session.UserId, order.Id));
			await _repository.InsertAsync(order);
		}

		public async Task<List<Order>> GetAllOrdersAsync()
		{
			var orders = await _repository.GetAllListAsync();
			return orders;
		}

		public async Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _repository.GetAsync(orderId);
			return order;
		}
	}
}