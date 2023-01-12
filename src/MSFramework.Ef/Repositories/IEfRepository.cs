using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef.Repositories;

public interface IEfRepository : IRepository
{
    DbContextBase GetDbContext();
}
