using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// Enumeration 类型映射扩展，将枚举对象映射为数据库 varchar 列
/// </summary>
public static class EnumerationPropertyExtensions
{
    // private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorInfoCache = new();

    /// <summary>
    /// 将 Enumeration 子类映射为数据库 varchar 列（已废弃，框架会自动处理）
    /// </summary>
    /// <param name="builder">属性构建器</param>
    /// <typeparam name="TProperty">Enumeration 子类类型</typeparam>
    /// <returns>属性构建器</returns>
    [Obsolete("枚举类型会自动设置")]
    public static PropertyBuilder<TProperty> UseEnumeration<TProperty>(this PropertyBuilder<TProperty> builder)
        where TProperty : Enumeration
    {
        // var type = typeof(TProperty);
        // var constructorInfo = ConstructorInfoCache.GetOrAdd(type, t =>
        // {
        //     var v = t.GetTypeInfo().DeclaredConstructors
        //         .FirstOrDefault(x =>
        //             x.GetParameters().Length == 2 && x.GetParameters()
        //                 .All(y => y.ParameterType == typeof(string)));
        //     return v;
        // });
        //
        // builder.HasConversion(new ValueConverter<TProperty, string>(
        //     v => v.Id,
        //     v => constructorInfo.Invoke(new object[] { v, v }) as TProperty));
        return builder;
    }
}
