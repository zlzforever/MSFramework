using System.Linq;
using Microsoft.EntityFrameworkCore;
using OrderAggregate = Ordering.Domain.AggregateRoots.Order.Order;

namespace Ordering.Infrastructure.Repositories.Order;

public partial class OrderRepository
{
    protected override IQueryable<OrderAggregate> BuildQueryable(DbSet<OrderAggregate> dbSet) =>
        dbSet.Include(x => x.Items);

    public void B()
    {
    }
}
