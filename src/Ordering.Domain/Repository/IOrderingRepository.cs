using System;
using System.Threading.Tasks;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IOrderingRepository : IRepository
	{
		Task<Order> GetAsync(Guid orderId);

		Task DeleteAsync(Order order);

		Task InsertAsync(Order order);
	}
}