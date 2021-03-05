using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IProductRepository : IRepository<Product, ObjectId>, IScopeDependency
	{
		Product GetFirst();
		Task<PagedResult<Product>> PagedQueryAsync(int page, int limit);
	}
}