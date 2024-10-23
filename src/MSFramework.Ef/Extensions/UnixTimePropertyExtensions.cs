using System;
using MicroserviceFramework.Ef.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class UnixTimePropertyExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static PropertyBuilder<DateTimeOffset?> UseUnixTime(this PropertyBuilder<DateTimeOffset?> builder,
        bool milliseconds = false)
    {
        // var converter = new ValueConverter<DateTimeOffset?, long?>(
        //     v => v.HasValue
        //         ? milliseconds ? v.Value.ToUnixTimeMilliseconds() : v.Value.ToUnixTimeSeconds()
        //         : default,
        //     v => v.HasValue
        //         ? milliseconds
        //             ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value).ToLocalTime()
        //             : DateTimeOffset.FromUnixTimeSeconds(v.Value).ToLocalTime()
        //         : default);
        // builder.Metadata.SetValueConverter(converter);
        builder.Metadata.SetValueConverter(new NullableDateTimeOffsetToLongConverter(milliseconds));
        builder.HasColumnType("bigint");
        builder.IsRequired(false);
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static PropertyBuilder<DateTimeOffset> UseUnixTime(this PropertyBuilder<DateTimeOffset> builder,
        bool milliseconds = false)
    {
        builder.IsRequired();
        builder.HasColumnType("bigint");
        builder.HasDefaultValue(DateTimeOffset.UnixEpoch);
        builder.Metadata.SetValueConverter(new DateTimeOffsetToLongConverter(milliseconds));
        return builder;
    }
}
