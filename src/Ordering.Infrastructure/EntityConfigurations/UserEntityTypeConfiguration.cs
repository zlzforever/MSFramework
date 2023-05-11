using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class UserEntityTypeConfiguration
    : EntityTypeConfigurationBase<User, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureDefaultIdentifier(builder);

        builder.ToTable("external_user");
        builder.Property(x => x.Name).HasMaxLength(256);
    }
}
