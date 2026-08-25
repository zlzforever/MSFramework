using System;
using MicroserviceFramework.Ef.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// DateTimeOffset Unix 时间戳映射扩展，将日期时间属性存储为 bigint 类型
/// </summary>
public static class UnixTimePropertyExtensions
{
    /// <summary>
    /// 将可空 DateTimeOffset 属性映射为 bigint Unix 时间戳
    /// </summary>
    /// <param name="builder">属性构建器</param>
    /// <param name="milliseconds">是否使用毫秒级时间戳，默认秒级</param>
    /// <returns>属性构建器</returns>
    public static PropertyBuilder<DateTimeOffset?> UseUnixTime(this PropertyBuilder<DateTimeOffset?> builder,
        bool milliseconds = false)
    {
        builder.Metadata.SetValueConverter(milliseconds
            ? new NullableDateTimeOffsetToUnixMsConverter()
            : new NullableDateTimeOffsetToUnixSecondsConverter());
        builder.HasColumnType("bigint");
        builder.IsRequired(false);
        return builder;
    }

    /// <summary>
    /// 将不可空 DateTimeOffset 属性映射为 bigint Unix 时间戳
    /// </summary>
    /// <param name="builder">属性构建器</param>
    /// <param name="milliseconds">是否使用毫秒级时间戳，默认秒级</param>
    /// <returns>属性构建器</returns>
    public static PropertyBuilder<DateTimeOffset> UseUnixTime(this PropertyBuilder<DateTimeOffset> builder,
        bool milliseconds = false)
    {
        builder.IsRequired();
        builder.HasColumnType("bigint");
        builder.HasDefaultValue(DateTimeOffset.UnixEpoch);
        builder.Metadata.SetValueConverter(milliseconds
            ? new DateTimeOffsetToUnixMsConverter()
            : new DateTimeOffsetToUnixSecondsConverter());
        return builder;
    }
}
