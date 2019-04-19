using System;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Repository
{
	public interface IOrderingRepository : IRepository<Order, Guid>
	{
	}
}