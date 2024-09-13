using System;
using MicroserviceFramework.Common;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class JsonPropertyBuilderExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="databaseType"></param>
    /// <param name="jsonHelper"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> UseJson<TProperty>(
        this PropertyBuilder<TProperty> builder,
        JsonDataType databaseType = JsonDataType.JSONB) where TProperty : class
    {
        var comparer = CreateValueComparer<TProperty>(typeof(TProperty));
        var propertyBuilder = builder
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
        propertyBuilder.HasConversion(x => Defaults.JsonSerializer.Serialize(x),
            x => Defaults.JsonSerializer.Deserialize<TProperty>(x));
        // var converter = new JsonToStringConverter<TProperty>();
        // propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        return propertyBuilder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="fieldType">JSON field 的数据类型。比如属性是 IReadonlyList, 其 field 可能定义为 HashSet
    /// 因此反序列化成 targetType 并使用 as 转换，肯定能转换</param>
    /// <param name="databaseType"></param>
    /// <param name="jsonHelper"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> UseJson<TProperty>(
        this PropertyBuilder<TProperty> builder,
        Type fieldType,
        JsonDataType databaseType = JsonDataType.JSONB, IJsonSerializer jsonHelper = null)
        where TProperty : class
    {
        Check.NotNull(fieldType, nameof(fieldType));
        var comparer = CreateValueComparer<TProperty>(fieldType);
        var propertyBuilder = builder
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
        propertyBuilder.HasConversion(x => Defaults.JsonSerializer.Serialize(x),
            x => Defaults.JsonSerializer.Deserialize(x, fieldType) as TProperty);
        // var converterType = typeof(JsonToStringConverter<>).MakeGenericType(fieldType);
        // var converter = (ValueConverter)Activator.CreateInstance(converterType);
        // propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return builder;
    }

    // public static PropertyBuilder<TProperty> UseJson<TProperty>(
    //     this PropertyBuilder<TProperty> builder,
    //     Type fieldType,
    //     IJsonSerializer jsonHelper, JsonDataType databaseType = JsonDataType.JSON)
    //     where TProperty : class
    // {
    //     builder.UseJson(fieldType, databaseType, jsonHelper);
    //     return builder;
    // }

    private static ValueComparer<TProperty> CreateValueComparer<TProperty>(
        Type fieldType)
        where TProperty : class
    {
        return new ValueComparer<TProperty>
        (
            (l, r) => Defaults.JsonSerializer.Serialize(l) ==
                      Defaults.JsonSerializer.Serialize(r),
            v => v == null ? 0 : Defaults.JsonSerializer.Serialize(v).GetHashCode(),
            v => Defaults.JsonSerializer.Deserialize(Defaults.JsonSerializer.Serialize(v), fieldType) as TProperty);
    }
}
