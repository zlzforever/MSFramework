using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Aggregates;

namespace Template.Infrastructure.EntityConfigurations;

/// <summary>商品 MySQL 映射，依赖框架审计、软删除与枚举约定，不配置数据库级联删除。</summary>
public class ProductConfiguration : EntityTypeConfigurationBase<Product, TemplateDbContext>
{
    /// <summary>配置名称、价格、类别及查询索引，其余列由框架约定补齐。</summary>
    /// <param name="builder">商品映射构建器，不允许为空。</param>
    /// <exception cref="ArgumentNullException">构建器为空。</exception>
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Property(product => product.Name).IsRequired().HasMaxLength(100);
        builder.Property(product => product.Price).IsRequired();
        // 枚举转换与审计字段由当前框架约定自动补齐，不调用已废弃的配置扩展。
        builder.HasIndex(product => product.Name);
        builder.HasIndex(product => product.CreationTime);
    }
}
