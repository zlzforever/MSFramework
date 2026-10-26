using System;
using System.Linq;
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
    /// 使用 <see cref="Utf8JsonReader.GetString"/> 读取值，内部按
    /// <see cref="Utf8JsonReader.HasValueSequence"/> 自动选择 ValueSpan/ValueSequence，
    /// 可正确处理值跨缓冲区段边界（如 PipeReader 分块读取）的场景。
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>反序列化后的 Enumeration 实例</returns>
    /// <exception cref="JsonException">输入不是字符串，或值不是目标枚举的合法 Id 时抛出</exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("枚举值必须为字符串。");
        }

        var value = reader.GetString();
        return Enumeration.GetAll(typeToConvert)
            .FirstOrDefault(item => string.Equals(item.Id, value, StringComparison.Ordinal)) is T result
            ? result
            : throw new JsonException($"枚举类型 {typeToConvert.Name} 不包含指定值。");
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
