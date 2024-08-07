using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderItemEntityTypeConfiguration
    : EntityTypeConfigurationBase<OrderItem, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        ConfigureDefaultIdentifier(builder);


        builder.OwnsOne(x => x.Product, y =>
        {
            y.Property(x => x.ProductId).HasColumnName("ProductId").HasMaxLength(36).IsRequired();
            y.Property(x => x.Name).HasMaxLength(255).IsRequired();
            y.Property(x => x.PictureUrl).HasMaxLength(300).IsRequired(false);

            y.HasIndex(x => x.ProductId);
        });
        builder.Property(x => x.Discount);
        builder.Property(x => x.Units);
        builder.Property(x => x.UnitPrice);
    }
}
