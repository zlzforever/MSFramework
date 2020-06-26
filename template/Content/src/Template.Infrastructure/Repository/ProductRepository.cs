using System.Linq;
using MSFramework.Ef;
using MSFramework.Ef.Repository;
using Template.Domain.AggregateRoot;
using Template.Domain.Repository;

namespace Template.Infrastructure.Repository
{
	public class ProductRepository
		: EfRepository<Product>, IProductRepository
	{
		public ProductRepository(DbContextFactory context) : base(context)
		{
		}

		public Product GetByName(string name)
		{
			return CurrentSet.FirstOrDefault(x => x.Name == name);
		}
	}
}