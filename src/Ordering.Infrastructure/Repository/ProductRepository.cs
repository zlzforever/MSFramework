using System.Linq;
using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.Ef;
using MSFramework.Ef.Repository;
using MSFramework.Extensions;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class ProductRepository : EfRepository<Product>, IProductRepository
	{
		public ProductRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public Product GetFirst()
		{
			return CurrentSet.FirstOrDefault();
		}

		public async Task<PagedResult<Product>> PagedQueryAsync(int page, int limit)
		{
			return await CurrentSet.PagedQueryAsync(page, limit);
		}
	}
}