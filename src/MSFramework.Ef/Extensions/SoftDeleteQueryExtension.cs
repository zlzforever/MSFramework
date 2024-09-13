using System;
using System.Linq.Expressions;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class SoftDeleteQueryExtension
{
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
    /// <param name="_"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static LambdaExpression GetSoftDeleteQueryFilter(Type type)
    {
        var method = typeof(SoftDeleteQueryExtension)
            .GetMethod(nameof(GetDeleteFilter),
                BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            throw new ArgumentException("GetSoftDeleteFilter method not found");
        }

        var methodToCall = method.MakeGenericMethod(type);
        var filter = methodToCall.Invoke(null, Array.Empty<object>());
        return (LambdaExpression)filter;
    }

    private static LambdaExpression GetDeleteFilter<TEntity>()
        where TEntity : class, IDeletion
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}
