using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class ProductConfiguration
    : EntityTypeConfigurationBase<Product, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Name).HasMaxLength(256);

        builder.ConfigureCreation();
    }
}
