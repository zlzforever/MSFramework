using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
/// <see cref="Enumeration"/> 派生类型的 JSON 转换器，序列化为 Id（字符串）。
/// </summary>
/// <typeparam name="T">继承自 <see cref="Enumeration"/> 的具体类型</typeparam>
public class EnumerationJsonConverter<T> : JsonConverter<T> where T : Enumeration
{
    /// <summary>
    /// 从 JSON 字符串反序列化，通过 <see cref="Enumeration.Parse"/> 匹配 Id。
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 Enumeration 实例</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
        return Enumeration.Parse(typeToConvert, value) as T;
    }

    /// <summary>
    /// 序列化为 Id 字符串。
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">待序列化的 Enumeration</param>
    /// <param name="options">序列化选项</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Id);
    }
}
