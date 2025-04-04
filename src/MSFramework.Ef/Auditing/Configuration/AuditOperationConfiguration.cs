using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

internal class AuditOperationConfiguration
{
    internal static readonly AuditOperationConfiguration Instance = new();

    public void Configure(EntityTypeBuilder<AuditOperation> builder)
    {
        builder.HasMany(x => x.Entities).WithOne(x => x.Operation);

        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.IP).HasMaxLength(256);
        builder.Property(x => x.DeviceId).HasMaxLength(36);
        builder.Property(x => x.DeviceModel).HasMaxLength(256);
        builder.Property(x => x.Lat);
        builder.Property(x => x.Lng);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.Path).HasMaxLength(1024);
        builder.Property(x => x.Elapsed);
        builder.Property(x => x.EndTime).UseUnixTime();
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.ConfigureCreation();

        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.EndTime);
    }
}
