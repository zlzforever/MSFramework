using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Ordering.Application;

namespace Ordering.API;

public class EfDbContextFactory(DbContextFactory dbContextFactory) : IDbContextFactory
{
    public DbContext GetDbContext<TEntity>()
    {
        return dbContextFactory.GetDbContext<TEntity>();
    }
}
