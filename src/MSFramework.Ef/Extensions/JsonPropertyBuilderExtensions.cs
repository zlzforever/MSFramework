using System;
using MicroserviceFramework.Common;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// JSON 属性映射扩展，ValueComparer 使用 UTF-8 字节流比较替代字符串比较。
/// </summary>
public static class JsonPropertyBuilderExtensions
{
    /// <param name="builder"></param>
    /// <typeparam name="TProperty"></typeparam>
    extension<TProperty>(PropertyBuilder<TProperty> builder) where TProperty : class
    {
        /// <summary>
        /// 将属性配置为 JSON 列存储，自动处理序列化/反序列化
        /// </summary>
        /// <param name="databaseType">数据库 JSON 类型，默认 JSONB</param>
        /// <returns>属性构建器</returns>
        public PropertyBuilder<TProperty> UseJson(JsonDataType databaseType = JsonDataType.JSONB)
        {
            var propertyBuilder = builder
                .UsePropertyAccessMode(PropertyAccessMode.PreferField)
                .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
            propertyBuilder.HasConversion(x => Defaults.JsonSerializer.Serialize(x),
                x => Defaults.JsonSerializer.Deserialize<TProperty>(x));
            builder.Metadata.SetProviderValueComparer(
                new ValueComparer<string>(
                    // 1. Equals表达式：使用序数对比，性能最高，无文化敏感问题，完全匹配JSON字节对比语义
                    equalsExpression: (a, b) => string.Equals(a, b, StringComparison.Ordinal),
                    // 2. HashCode表达式：null统一返回0，非null用字符串默认哈希，保证与Equals一致
                    hashCodeExpression: s => s == null ? 0 : s.GetHashCode(),
                    // 3. Snapshot表达式：字符串是不可变类型，直接返回自身即可，零拷贝开销
                    snapshotExpression: s => s
                )
            );
            // propertyBuilder.Metadata.SetValueComparer(new JsonStreamValueComparer<TProperty>());
            return propertyBuilder;
        }

        /// <summary>
        /// 将属性配置为 JSON 列存储，支持指定运行时类型以处理接口/抽象类型属性
        /// </summary>
        /// <param name="fieldType">运行时实际类型。当属性声明为接口或抽象类型时，传入具体实现类型用于反序列化</param>
        /// <param name="databaseType">数据库 JSON 类型，默认 JSONB</param>
        /// <returns>属性构建器</returns>
        public PropertyBuilder<TProperty> UseJson(Type fieldType,
            JsonDataType databaseType = JsonDataType.JSONB)
        {
            Check.NotNull(fieldType, nameof(fieldType));

            // 数据类型
            var propertyBuilder = builder
                .UsePropertyAccessMode(PropertyAccessMode.PreferField)
                .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");

            // 1. 内存对象对比：变更追踪
            propertyBuilder.HasConversion(x => Defaults.JsonSerializer.Serialize(x),
                x => Defaults.JsonSerializer.Deserialize(x, fieldType) as TProperty);

            // 2. 数据库JSON字符串对比：查询等值匹配
            builder.Metadata.SetProviderValueComparer(
                new ValueComparer<string>(
                    // 1. Equals表达式：使用序数对比，性能最高，无文化敏感问题，完全匹配JSON字节对比语义
                    equalsExpression: (a, b) => string.Equals(a, b, StringComparison.Ordinal),
                    // 2. HashCode表达式：null统一返回0，非null用字符串默认哈希，保证与Equals一致
                    hashCodeExpression: s => s == null ? 0 : s.GetHashCode(),
                    // 3. Snapshot表达式：字符串是不可变类型，直接返回自身即可，零拷贝开销
                    snapshotExpression: s => s
                )
            );
            return builder;
        }
    }
}
