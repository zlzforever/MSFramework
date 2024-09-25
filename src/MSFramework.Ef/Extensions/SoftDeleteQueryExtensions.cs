using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class SoftDeleteQueryExtensions
{
    private static readonly ConcurrentDictionary<Type, LambdaExpression> MethodInfoCache = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="entityData"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddSoftDeleteQueryFilter(
        this IMutableEntityType entityData)
    {
        var filter = GetSoftDeleteQueryFilter(entityData.ClrType);
        entityData.SetQueryFilter(filter);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static LambdaExpression GetDeleteFilter<TEntity>()
        where TEntity : class, IDeletion
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}
