using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
/// <see cref="DateTimeOffset"/> 的 JSON 转换器，序列化为 Unix 时间戳秒数。
/// </summary>
public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// 从 JSON 反序列化，支持字符串和数字两种格式。
    /// </summary>
    /// <remarks>
    /// 字符串分支使用 <see cref="CultureInfo.InvariantCulture"/> + RoundtripKind 解析：
    /// 避免服务器文化（非公历/不同数字格式）导致解析漂移，与 <see cref="DateTimeJsonConverter"/>
    /// 保持同一解析语义；空字符串不再静默返回 MinValue，按解析失败约定抛 <see cref="JsonException"/>
    /// </remarks>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 DateTimeOffset</returns>
    /// <exception cref="JsonException">字符串为空或格式非法、数值超出可表示范围时抛出</exception>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                throw new JsonException("DateTimeOffset 字符串为空，无法解析");
            }

            try
            {
                return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            catch (FormatException ex)
            {
                throw new JsonException($"DateTimeOffset 字符串 '{value}' 格式非法", ex);
            }
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (!reader.TryGetInt64(out var v))
            {
                throw new JsonException("Unix 时间戳数值超出 Int64 范围，无法转换为 DateTimeOffset");
            }

            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(v);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // 数值超出 DateTimeOffset 可表示范围，按解析失败约定统一抛 JsonException
                throw new JsonException("Unix 时间戳数值超出 DateTimeOffset 可表示范围，无法转换为 DateTimeOffset", ex);
            }
        }

        throw new NotSupportedException("不支持的数据类型");
    }

    /// <summary>
    /// 序列化为 Unix 时间戳秒数。
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">待序列化的值</param>
    /// <param name="options">序列化选项</param>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
