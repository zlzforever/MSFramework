using System;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot.Buyer;

namespace Ordering.Domain.Repository
{
	public interface IBuyerRepository : IRepository<Buyer, Guid>
	{
	}
}