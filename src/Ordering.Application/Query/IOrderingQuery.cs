using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Query
{
	public interface IOrderingQuery : IScopeDependency
	{
		Task<List<Order>> GetAllOrdersAsync();

		Task<Order> GetOrderAsync(Guid orderId);
	}
}