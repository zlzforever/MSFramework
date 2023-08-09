using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

public class AuditPropertyConfiguration
    : IEntityTypeConfiguration<AuditProperty>
{
    public static readonly AuditPropertyConfiguration Instance = new();

    public void Configure(EntityTypeBuilder<AuditProperty> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.Name).HasMaxLength(255);
        builder.Property(x => x.Type).HasMaxLength(255);
        builder.Property(x => x.NewValue);
        builder.Property(x => x.OriginalValue);
    }
}
