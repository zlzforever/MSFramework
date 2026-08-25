// using System;
// using System.Collections.Concurrent;
// using MicroserviceFramework.Domain;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.Metadata.Internal;
//
// namespace MicroserviceFramework.Ef.Extensions;
//
// /// <summary>
// /// 在有 Pool 的情况下，收益很低
// /// </summary>
// public static class DbContextOptionsBuilderExtensions
// {
//     private static readonly ConcurrentDictionary<string, IModel> Models = new();
//
//     /// <summary>
//     /// 加载 EF Core 编译模型以优化启动性能
//     /// </summary>
//     /// <param name="builder">DbContext 选项构建器</param>
//     /// <param name="type">编译模型类型的全称</param>
//     public static void LoadModel(this DbContextOptionsBuilder builder, string type)
//     {
//         var model = Models.GetOrAdd(type, key =>
//         {
//             var property = Type.GetType(key)
//                 ?.GetProperty("Instance");
//
//             if (property == null)
//             {
//                 return null;
//             }
//
//             if (property.GetValue(null) is not IModel model)
//             {
//                 return null;
//             }
//
//             var entities = model.GetEntityTypes();
//             foreach (var entity in entities)
//             {
//                 if (!typeof(IDeletion).IsAssignableFrom(entity.ClrType))
//                 {
//                     continue;
//                 }
//
//                 var e = (RuntimeEntityType)entity;
// #pragma warning disable EF1001
//                 if (e.FindAnnotation(CoreAnnotationNames.QueryFilter) == null)
//                 {
//                     var filter = SoftDeleteQueryExtensions.GetSoftDeleteQueryFilter(e.ClrType);
//                     e.AddAnnotation(CoreAnnotationNames.QueryFilter, filter);
// #pragma warning restore EF1001
//                 }
//             }
//
//             return model;
//         });
//
//         if (model == null)
//         {
//             throw new NotSupportedException("未找到模型定义");
//         }
//
//         builder.UseModel(model);
//     }
// }
