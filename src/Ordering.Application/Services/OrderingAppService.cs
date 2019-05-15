using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.Domain.Repository;
using MSFramework.EventBus;
using Ordering.Application.DTO;
using Ordering.Application.Event;
using Ordering.Application.Query;
using Ordering.Domain.AggregateRoot;


namespace Ordering.Application.Services
{
	public class OrderingAppService : ApplicationServiceBase, IOrderingAppService
	{
		private readonly IEventBus _eventBus;
		private readonly IHttpClientFactory _httpClientFactory;

		public OrderingAppService(IMSFrameworkSession session, IEventBus eventBus,
			IHttpClientFactory httpClientFactory,
			ILogger<OrderingAppService> logger) : base(session, logger)
		{
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

			var item = await Session.GetAsync<Order>(orderId);
			item.Delete();
			Logger.LogInformation($"DELETED ORDER: {orderId}");
		}

		public async Task ChangeOrderAddress(ChangeOrderAddressDTO dto)
		{
			var item = await Session.GetAsync<Order>(dto.OrderId);
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
			await Session.TrackAsync(order);
		}
	}
}