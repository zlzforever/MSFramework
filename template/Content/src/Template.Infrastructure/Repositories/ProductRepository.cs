using System.Linq;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Infrastructure.Repositories
{
	public class ProductRepository
		: EfRepository<Product>, IProductRepository
	{
		public ProductRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public Product GetByName(string name)
		{
			return AggregateRootSet.FirstOrDefault(x => x.Name == name);
		}
	}
}