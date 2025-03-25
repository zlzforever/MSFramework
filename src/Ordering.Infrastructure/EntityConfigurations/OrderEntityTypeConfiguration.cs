using MicroserviceFramework.Common;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Infrastructure.EntityConfigurations;

public class OrderEntityTypeConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        ConfigureDefaultIdentifier(builder);

        builder.OwnsOne(o => o.Address, x =>
        {
            x.Property(y => y.City).HasMaxLength(200).IsRequired();
            x.Property(y => y.Country).HasMaxLength(50).IsRequired();
            x.Property(y => y.ZipCode).HasMaxLength(20).IsRequired();
            x.Property(y => y.State).HasMaxLength(200).IsRequired();
            x.Property(y => y.Street).HasMaxLength(200).IsRequired();
        });
        builder.Property(x => x.TestId);
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);
        builder.Property(x => x.BuyerId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ListJson).UseJson(typeof(HashSet<string>), JsonDataType.JSON);
        builder.Property(x => x.DictJson).UseJson(JsonDataType.JSON);
        builder.Property(x => x.Extras).UseJson(JsonDataType.JSON);

        builder.HasOne(x => x.Operator).WithMany();

        builder.HasMany(x => x.Items).WithOne(x => x.Order)
            .OnDelete(DeleteBehavior.ClientCascade);

        // var navigation = builder.Metadata.FindNavigation(nameof(Order.Items));
        //
        // // DDD Patterns comment:
        // //Set as field (New since EF 1.1) to access the OrderItem collection property through its field
        // navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ConfigureCreation();

        builder.HasIndex(x => x.CreationTime);
    }
}
