using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="dbContext"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static string GetTableName<TEntity>(this DbContext dbContext) where TEntity : class
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        return entityType?.GetTableName();
    }
}
