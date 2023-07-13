using System.Collections.Generic;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderEntityTypeConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        ConfigureDefaultIdentifier(builder);

        //Address value object persisted as owned entity type supported since EF Core 2.0
        var navigationBuilder = builder.OwnsOne(o => o.Address);
        navigationBuilder.Property(x => x.City).IsRequired();
        navigationBuilder.WithOwner();

        builder.Property(x => x.Description).IsRequired(false);
        builder.Property(x => x.BuyerId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.Status).UseEnumeration().HasMaxLength(255).IsRequired();
        // 若类型不一致，则需要主动设置
        builder.Property(x => x.ListJson).UseJson(typeof(HashSet<string>));
        builder.Property(x => x.DictJson).UseJson();
        builder.Property(x => x.Extras).UseJson();
        // builder.Property<string>("creator_id2").HasMaxLength(36);
        // builder.HasOne(x => x.Creator2).WithMany().HasForeignKey("creator_id2");
        builder.HasMany(x => x.Items).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.ClientCascade);
        // var navigation = builder.Metadata.FindNavigation(nameof(Order.Items));
        //
        // // DDD Patterns comment:
        // //Set as field (New since EF 1.1) to access the OrderItem collection property through its field
        // navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ConfigureCreation();
    }
}
