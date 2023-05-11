using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class ProductConfiguration
    : EntityTypeConfigurationBase<Product, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        ConfigureDefaultIdentifier(builder);
        
        builder.Property(x => x.Name).HasMaxLength(256);
    }
}
