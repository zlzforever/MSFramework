using System;
using MicroserviceFramework.Common;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

public static class JsonPropertyBuilderExtensions
{
    public static PropertyBuilder<TProperty> UseJson<TProperty>(
        this PropertyBuilder<TProperty> builder,
        JsonDataType databaseType = JsonDataType.JSONB, IJsonSerializer jsonHelper = null) where TProperty : class
    {
        var jsonSerializer = GetJsonSerializer(jsonHelper);

        var comparer = CreateValueComparer<TProperty>(jsonSerializer);
        var propertyBuilder = builder
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
        propertyBuilder.HasConversion(x => jsonSerializer.Serialize(x),
            x => jsonSerializer.Deserialize<TProperty>(x));
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
        var jsonSerializer = GetJsonSerializer(jsonHelper);
        var comparer = CreateValueComparer<TProperty>(jsonSerializer, fieldType);
        var propertyBuilder = builder
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
        propertyBuilder.HasConversion(x => jsonSerializer.Serialize(x),
            x => jsonSerializer.Deserialize(x, fieldType) as TProperty);
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

    private static ValueComparer<TProperty> CreateValueComparer<TProperty>(IJsonSerializer jsonHelper,
        Type fieldType = null)
        where TProperty : class
    {
        return new ValueComparer<TProperty>
        (
            (l, r) => jsonHelper.Serialize(l) ==
                      jsonHelper.Serialize(r),
            v => v == null ? 0 : jsonHelper.Serialize(v).GetHashCode(),
            v => jsonHelper.Deserialize(jsonHelper.Serialize(v),
                fieldType ?? typeof(TProperty)) as TProperty);
    }

    private static IJsonSerializer GetJsonSerializer(IJsonSerializer jsonSerializer = null)
    {
        return jsonSerializer ??
               (Defaults.JsonSerializer == null ? TextJsonSerializer.Create() : Defaults.JsonSerializer);
    }
}
