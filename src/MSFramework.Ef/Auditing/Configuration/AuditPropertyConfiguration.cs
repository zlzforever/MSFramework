using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

internal static class AuditPropertyConfiguration
{
    public static void Configure(EntityTypeBuilder<AuditProperty> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.Type).HasMaxLength(256);
        builder.Property(x => x.NewValue);
        builder.Property(x => x.OriginalValue);
    }
}
