using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// 软删除查询过滤器扩展，自动为 IDeletion 实体添加 IsDeleted 过滤条件
/// </summary>
public static class SoftDeleteQueryExtensions
{
    private static readonly ConcurrentDictionary<Type, LambdaExpression> MethodInfoCache = new();

    /// <summary>
    /// 为实体类型添加软删除查询过滤器，自动过滤 IsDeleted 为 true 的记录
    /// </summary>
    /// <param name="entityData">可变实体类型元数据</param>
    /// <exception cref="ArgumentException">无法获取过滤器表达式时抛出</exception>
    public static void AddSoftDeleteQueryFilter(
        this IMutableEntityType entityData)
    {
        var softDelete = GetSoftDeleteQueryFilter(entityData.ClrType);
        entityData.SetQueryFilter("SoftDelete", softDelete);
    }

    /// <summary>
    /// 获取指定实体类型的软删除筛选表达式
    /// </summary>
    /// <param name="type">实体类型</param>
    /// <returns>Lambda 表达式</returns>
    /// <exception cref="ArgumentException">无法获取 GetDeleteFilter 方法时抛出</exception>
    public static LambdaExpression GetSoftDeleteQueryFilter(Type type)
    {
        return MethodInfoCache.GetOrAdd(type, t =>
        {
            var method = typeof(SoftDeleteQueryExtensions)
                .GetMethod(nameof(GetDeleteFilter),
                    BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                throw new ArgumentException("GetSoftDeleteFilter method not found");
            }

            var methodToCall = method.MakeGenericMethod(t);
            var filter = methodToCall.Invoke(null, []);
            return (LambdaExpression)filter;
        });
    }

    /// <summary>
    /// 获取泛型软删除筛选表达式：x => !x.IsDeleted
    /// </summary>
    /// <typeparam name="TEntity">实现 IDeletion 接口的实体类型</typeparam>
    /// <returns>筛选表达式</returns>
    public static LambdaExpression GetDeleteFilter<TEntity>()
        where TEntity : class, IDeletion
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}
