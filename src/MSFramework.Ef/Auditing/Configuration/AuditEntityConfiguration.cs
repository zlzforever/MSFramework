using MicroserviceFramework.Auditing;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

public class AuditEntityConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    public static readonly AuditEntityConfiguration Instance = new();

    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.HasMany(x => x.Properties).WithOne(x => x.Entity);

        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.EntityId).HasMaxLength(255);
        builder.Property(x => x.Type).HasMaxLength(255);
        builder.Property(e => e.OperationType).HasMaxLength(255).UseEnumeration();

        builder.HasIndex(m => m.EntityId);
    }
}
