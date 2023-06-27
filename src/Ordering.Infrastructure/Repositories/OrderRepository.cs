using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories;

public class OrderRepository
    : EfRepository<Order, ObjectId>, IOrderingRepository
{
    public OrderRepository(DbContextFactory context) : base(context)
    {
        UseQuerySplittingBehavior = true;
    }
}
