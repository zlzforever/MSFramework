using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IOrderRepository : IScopeDependency
	{
		Task<Order> GetAsync(Guid orderId);

		Task InsertAsync(Order order);

		Task<List<Order>> GetAllListAsync();
	}
}