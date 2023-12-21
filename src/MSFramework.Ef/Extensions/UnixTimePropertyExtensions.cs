using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Extensions;

public static class UnixTimePropertyExtensions
{
    public static PropertyBuilder<DateTimeOffset?> UseUnixTime(this PropertyBuilder<DateTimeOffset?> builder,
        bool milliseconds = false)
    {
        builder.HasConversion(new ValueConverter<DateTimeOffset?, long?>(
            v => v.HasValue
                ? milliseconds ? v.Value.ToUnixTimeMilliseconds() : v.Value.ToUnixTimeSeconds()
                : default,
            v => v.HasValue
                ? milliseconds
                    ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value).ToLocalTime()
                    : DateTimeOffset.FromUnixTimeSeconds(v.Value).ToLocalTime()
                : default));
        return builder;
    }

    public static PropertyBuilder<DateTimeOffset> UseUnixTime(this PropertyBuilder<DateTimeOffset> builder,
        bool milliseconds = false)
    {
        builder.HasConversion(new ValueConverter<DateTimeOffset, long>(
            v => milliseconds ? v.ToUnixTimeMilliseconds() : v.ToUnixTimeSeconds(),
            v => milliseconds
                ? DateTimeOffset.FromUnixTimeMilliseconds(v)
                : DateTimeOffset.FromUnixTimeSeconds(v).ToLocalTime()));
        builder.IsRequired();
        builder.HasDefaultValue(DateTimeOffset.UnixEpoch);
        return builder;
    }
}
