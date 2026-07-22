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
    /// 从 JSON 字符串反序列化为 <see cref="ObjectId"/>，空字符串返回 <see cref="ObjectId.Empty"/>。
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 ObjectId</returns>
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        System.Diagnostics.Debug.Assert(typeToConvert == typeof(ObjectId));
        return reader.ValueSpan.Length != 0 ? new ObjectId(System.Text.Encoding.UTF8.GetString(reader.ValueSpan)) : ObjectId.Empty;
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
