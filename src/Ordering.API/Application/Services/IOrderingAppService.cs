using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Application;
using Ordering.API.Application.DTO;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.Services
{
	public interface IOrderingAppService : IApplicationService
	{
		Task DeleteOrder(Guid orderId);

		Task ChangeOrderAddress(ChangeOrderAddressDTO dto);

		Task CreateOrder(CreateOrderDTO dto);
		
		Task<List<Order>> GetAllOrdersAsync();

		Task<Order> GetOrderAsync(Guid orderId);
	}
}