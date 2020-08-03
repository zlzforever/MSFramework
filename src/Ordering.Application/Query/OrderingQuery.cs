using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Common;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Query
{
	public class OrderingQuery : IOrderingQuery
	{
		private readonly DbContext _context;
		private readonly DbSet<Order> _orderSet;

		public OrderingQuery(DbContextFactory dbContextFactory)
		{
			_context = dbContextFactory.GetDbContext<Order>();
			_orderSet = _context.Set<Order>();
		}

		public async Task<List<Order>> GetAllOrdersAsync()
		{
			var result = await _orderSet.ToListAsync();
			foreach (var item in result)
			{
				foreach (var orderItem in item.OrderItems)
				{
					Console.WriteLine(orderItem.ToString());
				}
			}

			return result;
		}

		public async Task<Order> GetOrderAsync(ObjectId orderId)
		{
			var order = await _orderSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
			return order;
		}
	}
}