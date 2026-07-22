#nullable enable
using System;
using System.Linq;
using MicroserviceFramework.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroserviceFramework.Serialization.Newtonsoft.Converters;

/// <summary>
///     Enumeration 类型与字符串/整数之间的 Newtonsoft.Json 转换器
/// </summary>
public class EnumerationConverter : JsonConverter
{
    /// <summary>
    ///     将 Enumeration 序列化为其 Id 值（字符串），null 则输出 null
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">Enumeration 值</param>
    /// <param name="serializer">序列化器</param>
    /// <exception cref="MicroserviceFrameworkException">类型不是 Enumeration 子类时抛出</exception>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else if (value.GetType().IsSubclassOf(typeof(Enumeration)))
        {
            writer.WriteValue(((Enumeration)value).Id);
        }
        else
        {
            throw new MicroserviceFrameworkException(122, " no support json output");
        }
    }

    /// <summary>
    ///     从 JSON 中读取值并解析为对应的 Enumeration 实例
    /// </summary>
    /// <param name="reader">JSON 读取器</param>
    /// <param name="objectType">目标 Enumeration 类型</param>
    /// <param name="existingValue">现有值</param>
    /// <param name="serializer">序列化器</param>
    /// <returns>解析后的 Enumeration 实例，无效值返回 null</returns>
    /// <exception cref="MicroserviceFrameworkException">值无法匹配任何枚举项时抛出</exception>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var value = token.ToString();
        // var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
        // var enumType = objectType;
        // if (isNullable)
        // {
        //     enumType = objectType.GetGenericArguments().FirstOrDefault();
        // }

        if (token.Type == JTokenType.None
            || token.Type == JTokenType.Null
            || token.Type == JTokenType.Undefined
            || string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            var enumeration = Enumeration.GetAll(objectType).FirstOrDefault(i => i.Id == value);
            if (enumeration != null)
            {
                return enumeration;
            }
        }
        catch (Exception ex)
        {
            // 异常数据，不允许绑定
            throw new MicroserviceFrameworkException(122, $"{reader.Path} 不支持绑定值 {value}", ex);
        }

        // 异常数据，不允许绑定
        throw new MicroserviceFrameworkException(122, $"{reader.Path} 不支持绑定值 {value}");
    }

    /// <summary>
    ///     判断指定类型是否为 Enumeration 子类
    /// </summary>
    /// <param name="objectType">要判断的类型</param>
    /// <returns>是 Enumeration 子类返回 true</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsSubclassOf(typeof(Enumeration));
    }
}
