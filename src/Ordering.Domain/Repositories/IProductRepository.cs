using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories;

public interface IProductRepository : IRepository<Product, string>, IScopeDependency
{
    Task<PaginationResult<Product>> PagedQueryAsync(int page, int limit);
}
