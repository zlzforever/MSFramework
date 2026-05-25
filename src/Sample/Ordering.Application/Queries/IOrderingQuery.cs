using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Application.Queries;

public interface IOrderingQuery : IScopeDependency
{
    Task<List<Order>> GetAllListAsync();

    Task<Order> GetAsync(string orderId);
}
