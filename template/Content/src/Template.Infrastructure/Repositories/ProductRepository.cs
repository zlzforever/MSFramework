using System.Threading.Tasks;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Infrastructure.Repositories
{
	public class ProductRepository
		: EfRepository<Product, ObjectId>, IProductRepository
	{
		public ProductRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public async Task<Product> GetByNameAsync(string name)
		{
			return await Store.FirstOrDefaultAsync(x => x.Name == name);
		}
	}
}