using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Application.Queries;

/// <summary>
/// Query 为只读，不使用 Repository
/// </summary>
public class OrderingQuery(IDbContextFactory dbContextFactory) : IOrderingQuery
{
    public async Task<List<Order>> GetAllListAsync()
    {
        var dbContext = dbContextFactory.GetDbContext<Order>().Set<Order>();
        return await dbContext.ToListAsync();
    }

    public async Task<Order> GetAsync(string orderId)
    {
        var dbContext = dbContextFactory.GetDbContext<Order>().Set<Order>();
        var order = await dbContext
            .Include(x => x.Items)
            .Include(x => x.Buyer)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId);

        return order;
    }
}
