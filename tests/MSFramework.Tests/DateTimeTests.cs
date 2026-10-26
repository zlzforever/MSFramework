using System;
using System.Text;
using System.Text.Json;
using MicroserviceFramework.Text.Json.Converters;
using Newtonsoft.Json;
using Xunit;
using JsonException = System.Text.Json.JsonException;

namespace MSFramework.Tests;

public class DateTimeTests
{
    [Fact]
    public void LocalSerialization()
    {
        // 2023-01-18 10:17:00(+8)
        var unixTime = 1674008220;
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        var local = dt.ToLocalTime();
        var json1 = JsonConvert.SerializeObject(local);
        var json2 = JsonConvert.SerializeObject(dt);

        // 同一时刻的两种偏移表示：序列化往返后绝对时刻不变，且各自保留原偏移量
        var roundTripLocal = JsonConvert.DeserializeObject<DateTimeOffset>(json1);
        var roundTripUtc = JsonConvert.DeserializeObject<DateTimeOffset>(json2);
        Assert.Equal(local.Offset, roundTripLocal.Offset);
        Assert.Equal(dt.Offset, roundTripUtc.Offset);
        Assert.Equal(unixTime, roundTripLocal.ToUnixTimeSeconds());
        Assert.Equal(unixTime, roundTripUtc.ToUnixTimeSeconds());
        // 同一时刻在本地时区序列化结果与 UTC 表示不同（偏移非零时区）
        if (local.Offset != TimeSpan.Zero)
        {
            Assert.NotEqual(json1, json2);
        }
    }

    [Fact]
    public void ToLocal()
    {
        var unixTime = 1689262630;
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        var local1 = dt.ToLocalTime();
        var local2 = local1.ToLocalTime();
        var local3 = local2.ToLocalTime();
        Assert.Equal(local1, local2);
        Assert.Equal(local1, local3);
    }

    [Fact]
    public void DateTimeJsonConverter_ReadsInt64Timestamp_WhenExceedsInt32()
    {
        // 99999999999 超过 int32 范围，旧实现静默回退 UnixEpoch，新实现必须正确解析
        var converter = new DateTimeJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("99999999999"));
        Assert.True(reader.Read());

        var result = converter.Read(ref reader, typeof(DateTime), null);

        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(99999999999), result);
    }

    [Fact]
    public void DateTimeJsonConverter_ThrowsJsonException_WhenTimestampExceedsInt64()
    {
        // 2^63 超出 Int64 范围，解析失败必须抛 JsonException 而非静默回退
        var converter = new DateTimeJsonConverter();
        var bytes = Encoding.UTF8.GetBytes("9223372036854775808");

        Assert.Throws<JsonException>(() =>
        {
            var reader = new Utf8JsonReader(bytes);
            Assert.True(reader.Read());
            converter.Read(ref reader, typeof(DateTime), null);
        });
    }

    [Fact]
    public void DateTimeOffsetJsonConverter_ReadsInt64Timestamp_WhenExceedsInt32()
    {
        var converter = new DateTimeOffsetJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("99999999999"));
        Assert.True(reader.Read());

        var result = converter.Read(ref reader, typeof(DateTimeOffset), null);

        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(99999999999), result);
    }

    [Fact]
    public void DateTimeOffsetJsonConverter_ThrowsJsonException_WhenTimestampExceedsInt64()
    {
        var converter = new DateTimeOffsetJsonConverter();
        var bytes = "9223372036854775808"u8.ToArray();

        Assert.Throws<JsonException>(() =>
        {
            var reader = new Utf8JsonReader(bytes);
            Assert.True(reader.Read());
            converter.Read(ref reader, typeof(DateTimeOffset), null);
        });
    }

    /// <summary>
    /// 无时区输入（如 "2026-10-26T12:00:00"）：保留墙钟时间——
    /// .ToLocalTime() 会把 Unspecified 当作 UTC 凭空偏移（Asia/Shanghai 下 12:00 被错转成 20:00），
    /// 与写出端（ToUnixTimeSeconds 把 Unspecified 按本地墙钟处理）的约定不一致
    /// </summary>
    [Fact]
    public void DateTimeJsonConverter_UnspecifiedInput_KeepsWallClock()
    {
        var converter = new DateTimeJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("\"2026-10-26T12:00:00\""));
        Assert.True(reader.Read());

        var result = converter.Read(ref reader, typeof(DateTime), null);

        Assert.Equal(new DateTime(2026, 10, 26, 12, 0, 0), result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    /// <summary>
    /// Z 结尾输入（UTC 语义）：转换为本地时间，绝对时刻与写出端 Unix 秒一致
    /// </summary>
    [Fact]
    public void DateTimeJsonConverter_UtcInput_ConvertsToLocal()
    {
        var converter = new DateTimeJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("\"2026-10-26T12:00:00Z\""));
        Assert.True(reader.Read());

        var result = converter.Read(ref reader, typeof(DateTime), null);

        Assert.Equal(DateTimeKind.Local, result.Kind);
        // 与写出端同一时刻的 Unix 秒（表达式随测试机时区自适应，不硬编码偏移）
        Assert.Equal(DateTimeOffset.Parse("2026-10-26T12:00:00Z").LocalDateTime, result);
    }

    /// <summary>
    /// 显式偏移输入（如 "+05:00"）：RoundtripKind 保留墙钟并标记 Local，
    /// 不再被 ToLocalTime 二次偏移
    /// </summary>
    [Fact]
    public void DateTimeJsonConverter_ExplicitOffsetInput_KeepsWallClockAsLocal()
    {
        var converter = new DateTimeJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("\"2026-10-26T12:00:00+05:00\""));
        Assert.True(reader.Read());

        var result = converter.Read(ref reader, typeof(DateTime), null);

        // RoundtripKind 语义：偏移字符串解析为 Local Kind 且墙钟保持不变
        Assert.Equal(DateTimeKind.Local, result.Kind);
        Assert.Equal(DateTimeOffset.Parse("2026-10-26T12:00:00+05:00").LocalDateTime, result);
    }
}
