using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.IntegrateService;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Event;
using Ordering.Domain;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.API.Application.Services
{
	public class OrderingAppService : ApplicationServiceBase, IOrderingAppService
	{
		private readonly IOrderReadRepository _readRepository;
		private readonly IOrderWriteRepository _writeRepository;
		private readonly IIntegrateService _integrateService;

		public OrderingAppService(IMSFrameworkSession session, IIntegrateService integrateService,
			IOrderReadRepository readRepository,
			IOrderWriteRepository writeRepository,
			ILogger<OrderingAppService> logger) : base(session, logger)
		{
			_readRepository = readRepository;
			_writeRepository = writeRepository;
			_integrateService = integrateService;
		}

		public async Task DeleteOrder(DeleteOrderDto dto)
		{
			var item = await _writeRepository.GetAsync(dto.OrderId);
			item.Delete();
			Logger.LogInformation($"DELETED ORDER: {dto.OrderId}");
		}

		public async Task ChangeOrderAddress(ChangeOrderAddressDTO dto)
		{
			var item = await _writeRepository.GetAsync(dto.OrderId);
			item.ChangeAddress(dto.NewAddress);
		}

		public async Task CreateOrder(CreateOrderDTO dto)
		{
			var order = new Order(
				dto.UserId,
				new Address(dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode),
				dto.Description,
				dto.OrderItems.Select(x => x.ToOrderItem()).ToList());
			await _integrateService.PublishIntegrateEventAsync(new OrderStartedEvent(Session.UserId, order.Id));
			await _writeRepository.InsertAsync(order);
		}

		public async Task<List<Order>> GetAllOrdersAsync()
		{
			var orders = await _readRepository.GetAllListAsync();
			return orders;
		}

		public async Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _readRepository.GetAsync(orderId);
			return order;
		}
	}
}