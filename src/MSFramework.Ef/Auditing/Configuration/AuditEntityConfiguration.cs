using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

internal class AuditEntityConfiguration
{
    internal static readonly AuditEntityConfiguration Instance = new();

    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.HasMany(x => x.Properties).WithOne(x => x.Entity);

        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.EntityId).HasMaxLength(36);
        builder.Property(x => x.Type).HasMaxLength(256);
        builder.Property(e => e.OperationType).HasMaxLength(256);

        builder.HasIndex(m => m.EntityId);
    }
}
