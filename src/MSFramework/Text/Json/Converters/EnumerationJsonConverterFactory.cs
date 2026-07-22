using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
/// <see cref="Enumeration"/> 派生类型的 <see cref="JsonConverterFactory"/>，
/// 自动为每个 <see cref="Enumeration"/> 子类创建 <see cref="EnumerationJsonConverter{T}"/>。
/// </summary>
public class EnumerationJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// 判断指定类型是否为 <see cref="Enumeration"/> 的派生类型。
    /// </summary>
    /// <param name="typeToConvert">待判断的类型</param>
    /// <returns>true 表示可以创建转换器</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Enumeration).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    /// 为指定类型创建 <see cref="EnumerationJsonConverter{T}"/> 实例。
    /// </summary>
    /// <param name="typeToConvert">目标类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>对应的 JsonConverter 实例</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeof(EnumerationJsonConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(type) as JsonConverter;
    }
}
