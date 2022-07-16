using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
    public interface IProductRepository : IRepository<Product, ObjectId>, IScopeDependency
    {
        Product GetFirst();
        Task<PagedResult<Product>> PagedQueryAsync(int page, int limit);
    }
}