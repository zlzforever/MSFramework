using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
/// <see cref="DateTime"/> 的 JSON 转换器，序列化为 Unix 时间戳秒数。
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// 从 JSON 反序列化，支持字符串和数字两种格式。
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 DateTime（本地时间）</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? DateTime.MinValue : DateTime.Parse(value);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (!reader.TryGetInt64(out var v))
            {
                throw new JsonException("Unix 时间戳数值超出 Int64 范围，无法转换为 DateTime");
            }

            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(v).LocalDateTime;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // 数值超出 DateTime 可表示范围，按解析失败约定统一抛 JsonException
                throw new JsonException("Unix 时间戳数值超出 DateTime 可表示范围，无法转换为 DateTime", ex);
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
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
