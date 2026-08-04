using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
/// MongoDB <see cref="ObjectId"/> 的 JSON 转换器，序列化为 24 位十六进制字符串。
/// </summary>
public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    /// <summary>
    /// 从 JSON 字符串反序列化为 <see cref="ObjectId"/>，null/空字符串返回 <see cref="ObjectId.Empty"/>。
    /// 使用 <see cref="Utf8JsonReader.GetString"/> 读取值，内部按
    /// <see cref="Utf8JsonReader.HasValueSequence"/> 自动选择 ValueSpan/ValueSequence，
    /// 可正确处理值跨缓冲区段边界（如 PipeReader 分块读取）的场景，避免 ValueSpan 为空时静默丢失数据。
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 ObjectId，null/空字符串返回 <see cref="ObjectId.Empty"/></returns>
    /// <exception cref="FormatException">值不是合法的 24 位十六进制 ObjectId 字符串时抛出</exception>
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));
        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
    }

    /// <summary>
    /// 序列化为 24 位十六进制字符串。
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">待序列化的 ObjectId</param>
    /// <param name="options">序列化选项</param>
    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
