using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Domain.Repository
{
	public interface IProductRepository : IRepository<Product>, IScopeDependency
	{
		Product GetFirst();
		Task<PagedResult<Product>> PagedQueryAsync(int page, int limit);
	}
}