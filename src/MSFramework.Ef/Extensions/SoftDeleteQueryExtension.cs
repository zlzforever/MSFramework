using System;
using System.Linq.Expressions;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroserviceFramework.Ef.Extensions;

public static class SoftDeleteQueryExtension
{
    public static void AddSoftDeleteQueryFilter(
        this IMutableEntityType entityData)
    {
        var method = typeof(SoftDeleteQueryExtension)
            .GetMethod(nameof(GetDeleteFilter),
                BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            throw new ArgumentException("GetSoftDeleteFilter method not found");
        }

        var methodToCall = method.MakeGenericMethod(entityData.ClrType);
        var filter = methodToCall.Invoke(null, Array.Empty<object>());
        entityData.SetQueryFilter((LambdaExpression)filter);
    }

    private static LambdaExpression GetDeleteFilter<TEntity>()
        where TEntity : class, IDeletion
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}
