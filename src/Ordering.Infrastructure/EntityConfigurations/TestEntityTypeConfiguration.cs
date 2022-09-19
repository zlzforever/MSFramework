using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class TestEntityTypeConfiguration
    : EntityTypeConfigurationBase<TestEntity, OrderingContext2>
{
    public override void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(256);
    }
}
