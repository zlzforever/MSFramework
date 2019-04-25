using System;
using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IOrderReadRepository : IReadRepository<Order, Guid>, IScopeDependency
	{
	}
}