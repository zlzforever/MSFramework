using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions;

public static class DbContextExtensions
{
    public static string GetTableName<TEntity>(this DbContext dbContext) where TEntity : class
    {
        var dbSet = dbContext.Set<TEntity>();
        return dbSet.EntityType.GetTableName();
    }
}
