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
        => v => milliseconds ? DateTimeOffset.FromUnixTimeMilliseconds(v) : DateTimeOffset.FromUnixTimeSeconds(v);
}

/// <summary>
///
/// </summary>
public class NullableDateTimeOffsetToLongConverter(bool milliseconds = false)
    : ValueConverter<DateTimeOffset?, long>(ToLong(milliseconds), ToDateTimeOffset(milliseconds))
{
    private static Expression<Func<DateTimeOffset?, long>> ToLong(bool milliseconds = false)
        => v => v.HasValue ? (milliseconds ? v.Value.ToUnixTimeMilliseconds() : v.Value.ToUnixTimeSeconds()) : 0;

    private static Expression<Func<long, DateTimeOffset?>> ToDateTimeOffset(bool milliseconds = false)
        => v => v <= 0 ? null
            : milliseconds ? DateTimeOffset.FromUnixTimeMilliseconds(v)
            : DateTimeOffset.FromUnixTimeSeconds(v);
}
