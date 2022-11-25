using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class TestEntityTypeConfiguration
    : EntityTypeConfigurationBase<TestEntity2, OrderingContext2>
{
    public override void Configure(EntityTypeBuilder<TestEntity2> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasMaxLength(256);
    }
}
