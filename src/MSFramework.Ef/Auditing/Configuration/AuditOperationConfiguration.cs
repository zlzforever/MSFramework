using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

internal static class AuditOperationConfiguration
{
    public static void Configure(EntityTypeBuilder<AuditOperation> builder)
    {
        builder.HasMany(x => x.Entities).WithOne(x => x.Operation);

        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
        builder.Property(x => x.IP).HasMaxLength(256);
        builder.Property(x => x.Method).HasMaxLength(12);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.Path);
        builder.Property(x => x.Elapsed);
        builder.Property(x => x.EndTime).UseUnixTime();
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.QueryString);

        builder.Property(x => x.DeviceId).HasMaxLength(36);
        builder.Property(x => x.DeviceModel).HasMaxLength(256);
        builder.Property(x => x.Lat).HasPrecision(11, 8);
        builder.Property(x => x.Lng).HasPrecision(11, 8);
        builder.Property(x => x.IMEI).HasMaxLength(64);
        builder.Property(x => x.Platform).HasMaxLength(32);
        builder.Property(x => x.Screen).HasMaxLength(32);
        builder.Property(x => x.OSVersion).HasMaxLength(32);
        builder.Property(x => x.LocationSource).HasMaxLength(32);
        builder.Property(x => x.Altitude);
        builder.Property(x => x.Battery);
        builder.Property(x => x.Signal);
        builder.Property(x => x.Accuracy);
        builder.Property(x => x.Bearing);
        builder.Property(x => x.Orientation);
        builder.Property(x => x.Emulator);

        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.EndTime);
    }
}
