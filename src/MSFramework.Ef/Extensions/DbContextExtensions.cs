using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// DbContext 扩展方法
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// 获取实体类型对应的数据库表名
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>数据库表名</returns>
    public static string GetTableName<TEntity>(this DbContext dbContext) where TEntity : class
    {
        // 内部是 O(1)，不需要再缓存了
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        return entityType?.GetTableName();
    }
}
