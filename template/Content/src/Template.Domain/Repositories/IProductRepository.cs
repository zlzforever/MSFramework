using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;

namespace Template.Domain.Repositories
{
	public interface IProductRepository : IRepository<Product, ObjectId>, IScopeDependency
	{
		Task<Product> GetByNameAsync(string name);
	}
}