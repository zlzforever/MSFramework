using MSFramework.DependencyInjection;
using MSFramework.Domain;
using Template.Domain.AggregateRoot;

namespace Template.Domain.Repository
{
	public interface IProductRepository : IRepository<Product>, IScopeDependency
	{
		Product GetByName(string name);
	}
}