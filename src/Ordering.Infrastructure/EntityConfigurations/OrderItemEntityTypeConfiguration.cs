using System;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderItemEntityTypeConfiguration : EntityTypeConfigurationBase<OrderItem, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        ConfigureDefaultIdentifier(builder);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property<decimal>("Discount").IsRequired();
        builder.Property<string>("ProductId").IsRequired();
        builder.Property<string>("ProductName").IsRequired();
        builder.Property<decimal>("UnitPrice").IsRequired();
        builder.Property<int>("Units").IsRequired();
        builder.Property<string>("PictureUrl").IsRequired(false);
        // builder.HasOne(x => x.Creator).WithMany().HasForeignKey("creator_id");
    }
}
