using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories;

public class UserInfoRepository(DbContextFactory dbContextFactory)
    : ExternalEntityRepository<UserInfo, string>(dbContextFactory), IUserInfoRepository
{
    public UserInfo Find(string id)
    {
        return Store.FirstOrDefault(x => x.Id == id);
    }
}
