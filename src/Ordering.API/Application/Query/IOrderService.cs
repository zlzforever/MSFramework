using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.Query
{
	public interface IOrderService
	{
		Task<List<Order>> GetAllOrdersAsync();

		Task<Order> GetOrderAsync(Guid orderId);
	}
}
