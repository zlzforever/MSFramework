using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;

namespace Template.Domain.Repositories
{
	public interface IProductRepository : IRepository<Product, ObjectId>, IScopeDependency
	{
		Task<Product> GetByNameAsync(string name);
	}
}