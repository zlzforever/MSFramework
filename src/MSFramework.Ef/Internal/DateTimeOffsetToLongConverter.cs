using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
///
/// </summary>
public class DateTimeOffsetToLongConverter(bool milliseconds = false)
    : ValueConverter<DateTimeOffset, long>(ToLong(milliseconds), ToDateTimeOffset(milliseconds))
{
    private static Expression<Func<DateTimeOffset, long>> ToLong(bool milliseconds = false)
        => v => milliseconds ? v.ToUnixTimeMilliseconds() : v.ToUnixTimeSeconds();

    private static Expression<Func<long, DateTimeOffset>> ToDateTimeOffset(bool milliseconds = false)
        => v => milliseconds
            ? DateTimeOffset.FromUnixTimeMilliseconds(v).ToLocalTime()
            : DateTimeOffset.FromUnixTimeSeconds(v).ToLocalTime();
}

/// <summary>
///
/// </summary>
public class NullableDateTimeOffsetToLongConverter(bool milliseconds = false)
    : ValueConverter<DateTimeOffset?, long?>(ToLong(milliseconds), ToDateTimeOffset(milliseconds))
{
    private static Expression<Func<DateTimeOffset?, long?>> ToLong(bool milliseconds = false)
        => v => v.HasValue ? (milliseconds ? v.Value.ToUnixTimeMilliseconds() : v.Value.ToUnixTimeSeconds()) : null;

    private static Expression<Func<long?, DateTimeOffset?>> ToDateTimeOffset(bool milliseconds = false)
        => v => !v.HasValue ? null
            : milliseconds ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value).ToLocalTime()
            : DateTimeOffset.FromUnixTimeSeconds(v.Value).ToLocalTime();
}
