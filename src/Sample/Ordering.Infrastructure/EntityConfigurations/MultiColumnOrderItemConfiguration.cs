using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots.CompositeKey;

namespace Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 多列复合主键订单项（方案 B：实体自身多属性作主键）实体映射配置。
/// 使用 EF 顶层标量多列 <c>HasKey(x =&gt; new { x.OrderId, x.ProductId })</c>，
/// EF Core 10 原生支持，配合无键仓储 <see cref="MicroserviceFramework.Domain.IRepository{TAggregateRoot}"/> 使用。
/// </summary>
public class MultiColumnOrderItemConfiguration : EntityTypeConfigurationBase<MultiColumnOrderItem, OrderingContext>
{
    /// <summary>
    /// 配置多列复合主键与业务列
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public override void Configure(EntityTypeBuilder<MultiColumnOrderItem> builder)
    {
        builder.HasKey(x => new { x.OrderId, x.ProductId });
        builder.Property(x => x.OrderId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.ProductId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity);
    }
}
