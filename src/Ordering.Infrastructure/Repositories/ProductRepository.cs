using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.Shared;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories
{
	public class ProductRepository : EfRepository<Product, ObjectId>, IProductRepository
	{
		public ProductRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public Product GetFirst()
		{
			return Store.FirstOrDefault();
		}

		public async Task<PagedResult<Product>> PagedQueryAsync(int page, int limit)
		{
			return await Store.PagedQueryAsync(page, limit);
		}
	}
}