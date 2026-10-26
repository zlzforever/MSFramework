using System;
using MicroserviceFramework.Common;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// JSON 属性映射扩展：将复杂属性（集合、对象等）序列化为 JSON 列存储，
/// 并配置变更追踪所需的深拷贝快照与序列化等值比较。
/// </summary>
public static class JsonPropertyBuilderExtensions
{
    /// <param name="builder">属性构建器</param>
    /// <typeparam name="TProperty">属性类型</typeparam>
    extension<TProperty>(PropertyBuilder<TProperty> builder) where TProperty : class
    {
        /// <summary>
        /// 将属性配置为 JSON 列存储，按属性声明类型序列化/反序列化。
        /// </summary>
        /// <remarks>
        /// 设计决策（勿在后续 review 中重复提出）：默认值 <see cref="JsonDataType.JSONB"/>
        /// 是有意保持的——SQL Server / MySQL 不存在 JSONB 列类型，使用默认值生成迁移时
        /// 会直接在建表阶段报列类型非法。这是可接受的失败形态：用户主动创建迁移即可看到错误，
        /// 按数据库显式传入 <see cref="JsonDataType.JSON"/>（样例实体配置均如此）即可修正，
        /// 框架不为各 provider 做隐式默认值推断（隐式推断反而让列类型决策不可见）。
        /// </remarks>
        /// <param name="databaseType">数据库 JSON 类型，默认 JSONB</param>
        /// <returns>属性构建器</returns>
        public PropertyBuilder<TProperty> UseJson(JsonDataType databaseType = JsonDataType.JSONB)
        {
            return builder.UseJson(typeof(TProperty), databaseType);
        }

        /// <summary>
        /// 将属性配置为 JSON 列存储，支持指定运行时类型以处理接口/抽象类型属性。
        /// </summary>
        /// <param name="fieldType">JSON 字段的数据类型。比如属性是 IReadOnlyList，其 field 可能定义为 HashSet——
        /// 反序列化成 fieldType 后强制转换回 TProperty；<b>必须</b>可赋值给属性类型，否则抛出</param>
        /// <param name="databaseType">数据库 JSON 类型，默认 JSONB</param>
        /// <returns>属性构建器</returns>
        /// <exception cref="ArgumentException">fieldType 不可赋值给属性类型
        /// （反序列化后的实例无法强转回 TProperty）时抛出</exception>
        /// <remarks>
        /// 变更追踪成本提示（TODO 待调整，见 CreateValueComparer）：ValueComparer 的相等比较与
        /// 快照均基于全量 JSON 序列化——大 JSON 属性 + 长会话实体会放大 DetectChanges 成本，
        /// 建议对结构明确的类型改用结构化相等（原生 Equals/GetHashCode）。
        /// </remarks>
        public PropertyBuilder<TProperty> UseJson(Type fieldType,
            JsonDataType databaseType = JsonDataType.JSONB)
        {
            Check.NotNull(fieldType, nameof(fieldType));

            if (!typeof(TProperty).IsAssignableFrom(fieldType))
            {
                throw new ArgumentException(
                    $"fieldType {fieldType} 不可赋值给属性类型 {typeof(TProperty)}——" +
                    "UseJson(fieldType) 用于接口/抽象属性指定运行时具体类型，" +
                    "反序列化后的实例必须能强转回属性类型", nameof(fieldType));
            }

            builder
                .UsePropertyAccessMode(PropertyAccessMode.PreferField)
                .HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB")
                .HasConversion(
                    x => Defaults.JsonSerializer.Serialize(x),
                    x => Deserialize<TProperty>(x, fieldType));

            // 设置模型属性元数据上的 ValueComparer（builder.Metadata 与 HasConversion 返回的构建器指向同一份属性元数据）。
            // 注意与 SetProviderValueComparer 的区别：模型 ValueComparer 服务于变更追踪
            // （快照深拷贝、新旧值等值比较），决定"属性是否变化、是否生成 UPDATE"；
            // Provider ValueComparer 服务于数据库提供程序层的值比较（如查询等值判断）。
            // 若只设置 ProviderComparer，可变对象（集合/字典）的新旧值对比可能永远"相等"、变更丢失。
            builder.Metadata.SetValueComparer(CreateValueComparer<TProperty>(fieldType));
            return builder;
        }
    }

    private static TProperty Deserialize<TProperty>(string json, Type fieldType)
    {
        var result = Defaults.JsonSerializer.Deserialize(json, fieldType);
        return result == null ? default : (TProperty)result;
    }

    // TODO(性能): comparer 全量序列化成本待调整——equals 每次比较序列化 2 次、hash 1 次、
    // 快照 1 次序列化 + 1 次反序列化，且按 JSON 文本比较对字典顺序/格式敏感；
    // 可选方案：结构化相等（原生 Equals/GetHashCode）或规范化比较（JsonNode.DeepEquals）。
    // 见 UseJson 的 remarks。
    /// <summary>
    /// equalsExpression：数据库存的是 JSON，最终都是要新、旧数据对比（即会有两份 JSON），无法优化的。除非永远认为是新数据。
    /// hashCodeExpression：在相同序列化条件下，如果数据没有变，hashCode 相同，不需要走 equalsExpression
    /// snapshotExpression：数据库存的是 JSON，因此和序列化设置强相关（私有字段、忽略属性、大小写、缩进、空格等），通过序列化和反序列化来实现深拷贝，保证一致性。
    /// </summary>
    /// <param name="fieldType"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
    private static ValueComparer<TProperty> CreateValueComparer<TProperty>(
        Type fieldType)
        where TProperty : class
    {
        return new ValueComparer<TProperty>
        (
            (left, right) => ContentEquals(left, right),
            instance => ComputeHashCode(instance),
            instance => CreateSnapshot(instance, fieldType)
        );
    }

    private static bool ContentEquals<TProperty>(TProperty left, TProperty right)
    {
        return Defaults.JsonSerializer.Serialize(left) == Defaults.JsonSerializer.Serialize(right);
    }

    private static int ComputeHashCode<TProperty>(TProperty instance)
    {
        return instance == null ? 0 : Defaults.JsonSerializer.Serialize(instance).GetHashCode();
    }

    private static TProperty CreateSnapshot<TProperty>(TProperty instance, Type fieldType)
    {
        var result = Defaults.JsonSerializer.Deserialize(Defaults.JsonSerializer.Serialize(instance), fieldType);
        return result == null ? default : (TProperty)result;
    }
}
