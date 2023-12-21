using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Extensions;

public static class EnumerationPropertyExtensions
{
    private static readonly Dictionary<Type, ConstructorInfo> ConstructorInfoCache = new();

    public static PropertyBuilder<TProperty> UseEnumeration<TProperty>(this PropertyBuilder<TProperty> builder)
        where TProperty : Enumeration
    {
        var type = typeof(TProperty);
        ConstructorInfo constructorInfo;
        if (ConstructorInfoCache.TryGetValue(type, out var c))
        {
            constructorInfo = c;
        }
        else
        {
            constructorInfo = typeof(TProperty).GetTypeInfo().DeclaredConstructors
                .FirstOrDefault(x =>
                    x.GetParameters().Length == 2 && x.GetParameters().All(y => y.ParameterType == typeof(string)));
            ConstructorInfoCache.Add(type, constructorInfo);
        }

        builder.HasConversion(new ValueConverter<TProperty, string>(
            v => v.Id,
            v => constructorInfo.Invoke(new object[] { v, v }) as TProperty));
        return builder;
    }
}
