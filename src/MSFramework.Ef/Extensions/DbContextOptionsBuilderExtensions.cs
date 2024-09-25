using System;
using System.Collections.Concurrent;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    private static readonly ConcurrentDictionary<string, IModel> Models = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    public static void LoadModel(this DbContextOptionsBuilder builder, string type)
    {
        var model = Models.GetOrAdd(type, key =>
        {
            var property = Type.GetType(key)
                ?.GetProperty("Instance");

            if (property == null)
            {
                return null;
            }

            if (property.GetValue(null) is not IModel model)
            {
                return null;
            }

            var entities = model.GetEntityTypes();
            foreach (var entity in entities)
            {
                if (!typeof(IDeletion).IsAssignableFrom(entity.ClrType))
                {
                    continue;
                }

                var e = (RuntimeEntityType)entity;
                var filter = SoftDeleteQueryExtensions.GetSoftDeleteQueryFilter(e.ClrType);
#pragma warning disable EF1001
                e.AddAnnotation(CoreAnnotationNames.QueryFilter, filter);
#pragma warning restore EF1001
            }

            return model;
        });

        if (model == null)
        {
            throw new NotSupportedException("未找到模型定义");
        }

        builder.UseModel(model);
    }
}
