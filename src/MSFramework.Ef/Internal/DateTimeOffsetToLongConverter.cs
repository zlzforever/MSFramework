using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// DateTimeOffset UTC 转 long Unix秒
/// </summary>
public class DateTimeOffsetToUnixSecondsConverter : ValueConverter<DateTimeOffset, long>
{
    /// <summary>
    ///
    /// </summary>
    public DateTimeOffsetToUnixSecondsConverter()
        : base(v => v.ToUnixTimeSeconds(), l => DateTimeOffset.FromUnixTimeSeconds(l))
    {
    }
}

/// <summary>DateTimeOffset UTC 转 long Unix毫秒</summary>
public class DateTimeOffsetToUnixMsConverter : ValueConverter<DateTimeOffset, long>
{
    /// <summary>
    ///
    /// </summary>
    public DateTimeOffsetToUnixMsConverter()
        : base(v => v.ToUnixTimeMilliseconds(), l => DateTimeOffset.FromUnixTimeMilliseconds(l))
    {
    }
}

/// <summary>
/// 可空 DateTimeOffset 到可空 long 的转换器，将可空日期时间存储为 Unix 时间戳（秒）
/// </summary>
public class NullableDateTimeOffsetToUnixSecondsConverter : ValueConverter<DateTimeOffset?, long?>
{
    /// <summary>
    ///
    /// </summary>
    public NullableDateTimeOffsetToUnixSecondsConverter()
        : base(v => v.HasValue ? v.Value.ToUnixTimeSeconds() : null,
            l => l.HasValue ? DateTimeOffset.FromUnixTimeSeconds(l.Value) : null)
    {
    }
}

/// <summary>可空 DateTimeOffset 到可空 long 的转换器，将可空日期时间存储为 Unix 时间戳（毫秒）</summary>
public class NullableDateTimeOffsetToUnixMsConverter : ValueConverter<DateTimeOffset?, long?>
{
    /// <summary>
    ///
    /// </summary>
    public NullableDateTimeOffsetToUnixMsConverter()
        : base(v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : null,
            l => l.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(l.Value) : null)
    {
    }
}
