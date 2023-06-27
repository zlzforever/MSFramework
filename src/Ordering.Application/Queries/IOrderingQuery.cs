using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Queries;

public interface IOrderingQuery : IScopeDependency
{
    Task<List<Order>> GetAllListAsync();

    Task<Order> GetAsync(ObjectId orderId);
}
