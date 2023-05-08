namespace MicroserviceFramework.Ef.Repositories;

public interface IEfRepository
{
    DbContextBase GetDbContext();
}
