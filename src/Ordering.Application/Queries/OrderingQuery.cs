using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Infrastructure;

namespace Ordering.Application.Queries;

public class OrderingQuery(OrderingContext context) : IOrderingQuery
{
    private readonly DbSet<Order> _orderSet = context.Set<Order>();

    public async Task<List<Order>> GetAllListAsync()
    {
        return await _orderSet.ToListAsync();
    }

    public async Task<Order> GetAsync(string orderId)
    {
        var order = await _orderSet
            .Include("Items")
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId);

        return order;
    }
}
