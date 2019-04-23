using Ordering.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.API.Application.Query
{
	public interface IOrderService
	{
		Task<List<Order>> GetAllOrdersAsync();

		Task<Order> GetOrderAsync(Guid orderId);
	}
}
