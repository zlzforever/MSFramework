using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Infrastructure;

namespace Ordering.Application.Queries
{
	public class PreBillLocker : IScopeDependency
	{
		
	}
	public class OrderingQuery : IOrderingQuery
	{
		private readonly DbSet<Order> _orderSet;

		public OrderingQuery(OrderingContext context)
		{
			_orderSet = context.Set<Order>();
		}

		public async Task<List<Order>> GetAllListAsync()
		{
			return await _orderSet.ToListAsync();
		}

		public async Task<Order> GetAsync(ObjectId orderId)
		{
			var order = await _orderSet
				.Include(x=>x.OrderItems)
				.FirstOrDefaultAsync(x => x.Id == orderId);
			return order;
		}
	}
}