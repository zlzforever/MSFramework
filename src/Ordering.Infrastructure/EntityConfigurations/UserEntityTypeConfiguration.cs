using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations;

public class UserEntityTypeConfiguration
    : EntityTypeConfigurationBase<UserInfo, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.ToTable("user", t => t.ExcludeFromMigrations());
        builder.Property(x => x.Name).HasMaxLength(256);
    }
}
