using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots.CompositeKey;

namespace Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 复合主键订单项（方案 A：值对象键）实体映射配置。
/// 使用 <see cref="EntityTypeBuilderExtensions.ConfigureCompositeKey{TEntity,TKey}"/>
/// 将 <see cref="OrderItemKey"/> 值对象经 ValueConverter 映射为单列主键
/// （EF Core 10 不支持复杂类型/owned 类型成员作主键，该能力自 EF 11 提供）。
/// </summary>
public class CompositeOrderItemConfiguration : EntityTypeConfigurationBase<CompositeOrderItem, OrderingContext>
{
    /// <summary>
    /// 配置复合主键值对象键映射与业务列
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public override void Configure(EntityTypeBuilder<CompositeOrderItem> builder)
    {
        builder.ConfigureCompositeKey(
            x => x.Id,
            key => key.ToString(),
            OrderItemKey.Parse);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity);
    }
}
