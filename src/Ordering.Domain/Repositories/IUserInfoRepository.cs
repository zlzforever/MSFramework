using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Domain.Repositories;

public interface IUserInfoRepository
    : IExternalEntityRepository<UserInfo, string>, IScopeDependency
{
    UserInfo Find(string id);
}
