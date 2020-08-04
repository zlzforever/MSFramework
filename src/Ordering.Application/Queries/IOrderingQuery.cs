using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Common;
using MSFramework.DependencyInjection;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Queries
{
	public interface IOrderingQuery : IScopeDependency
	{
		Task<List<Order>> GetAllOrdersAsync();

		Task<Order> GetOrderAsync(ObjectId orderId);
	}
}