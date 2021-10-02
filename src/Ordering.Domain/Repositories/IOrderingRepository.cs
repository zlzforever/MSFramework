using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories
{
	public interface IOrderingRepository : IRepository<Order, ObjectId>, IScopeDependency
	{
	}
}