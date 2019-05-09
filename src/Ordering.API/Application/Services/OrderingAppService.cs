using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Domain.Repository;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventBus;
using MSFramework.Serialization;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Event;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.Services
{
	public class OrderingAppService : ApplicationServiceBase, IOrderingAppService
	{
		private readonly EfRepository<Order, Guid> _repository;
		private readonly IEventBus _eventBus;
		private readonly IHttpClientFactory _httpClientFactory;

		public OrderingAppService(IMSFrameworkSession session, IEventBus eventBus,
			EfRepository<Order, Guid> repository,
			IHttpClientFactory httpClientFactory,
			ILogger<OrderingAppService> logger) : base(session, logger)
		{
			_repository = repository;
			_eventBus = eventBus;
			_httpClientFactory = httpClientFactory;
		}

		public async Task DeleteOrder(Guid orderId)
		{
// 			内部接口相互调用的例子			
//			var client = _httpClientFactory.CreateClient();
//			await SetBearer(client);
//			var json = await client.GetStringAsync("http://www.baidu.com");
//			var objects = Singleton<IJsonConvert>.Instance.DeserializeObject<Order>(json);

			var item = await _repository.GetAsync(orderId);
			item.Delete();
			await _repository.DeleteAsync(item);
			Logger.LogInformation($"DELETED ORDER: {orderId}");
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
			var orders = await _repository.AggregateRoots.AsNoTracking().ToListAsync();
			return orders;
		}

		public async Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _repository.AggregateRoots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
			return order;
		}
	}
}