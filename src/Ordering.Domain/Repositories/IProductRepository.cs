using System.Threading.Tasks;
using MSFramework.Common;
using MSFramework.DependencyInjection;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IProductRepository : IRepository<Product>, IScopeDependency
	{
		Product GetFirst();
		Task<PagedResult<Product>> PagedQueryAsync(int page, int limit);
	}
}