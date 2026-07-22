using System;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft.Converters;

/// <summary>
///     MongoDB ObjectId 与字符串之间的 Newtonsoft.Json 转换器
/// </summary>
public class ObjectIdConverter : JsonConverter<ObjectId>
{
    /// <summary>
    ///     将 ObjectId 序列化为字符串
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">ObjectId 值</param>
    /// <param name="serializer">序列化器</param>
    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    /// <summary>
    ///     从 JSON 字符串读取并解析为 ObjectId，空值或 null 返回 ObjectId.Empty
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="objectType">目标类型</param>
    /// <param name="existingValue">现有值</param>
    /// <param name="hasExistingValue">是否已有值</param>
    /// <param name="serializer">序列化器</param>
    /// <returns>解析后的 ObjectId</returns>
    /// <exception cref="JsonSerializationException">JSON 标记类型不是字符串时抛出</exception>
    public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return ObjectId.Empty;
        }

        if (reader.TokenType != JsonToken.String)
        {
            throw new JsonSerializationException($"Expected String but got {reader.TokenType}.");
        }

        if (reader.Value == null || (string)reader.Value == string.Empty)
        {
            return ObjectId.Empty;
        }

        return new ObjectId((string)reader.Value);
    }
}
