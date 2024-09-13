using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories;

public interface IOrderingRepository : IRepository<Order, string>, IScopeDependency;
