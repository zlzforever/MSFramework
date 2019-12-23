using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

		public Task<List<Order>> GetAllOrdersAsync()
		{
			return _orderSet.AsNoTracking().ToListAsync();
		}

		public async Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _orderSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
			return order;
		}
	}
}