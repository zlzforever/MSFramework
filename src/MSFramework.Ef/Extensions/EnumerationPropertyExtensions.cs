using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class EnumerationPropertyExtensions
{
    // private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorInfoCache = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
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
