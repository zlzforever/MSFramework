using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using Template.Domain.Aggregates.Project;

namespace Template.Domain.Repositories
{
	public interface IProductRepository : IRepository<Product>, IScopeDependency
	{
		Product GetByName(string name);
	}
}