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
}
