using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Linq.Expression;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories;

public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository
{
    public Product GetFirst()
    {
        return Store.FirstOrDefault();
    }

    public async Task<PaginationResult<Product>> PagedQueryAsync(int page, int limit)
    {
        return await Store.PagedQueryAsync(page, limit);
    }
}
