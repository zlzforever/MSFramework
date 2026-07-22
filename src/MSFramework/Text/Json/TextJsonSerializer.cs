using System;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

/// <summary>
/// 基于 <see cref="System.Text.Json"/> 的 JSON 序列化实现
/// </summary>
/// <param name="options">JSON 序列化选项</param>
public class TextJsonSerializer(JsonSerializerOptions options) : IJsonSerializer
{
    /// <summary>
    /// 创建默认的 JSON 序列化选项，预配置内建转换器
    /// </summary>
    /// <returns>默认配置的 <see cref="JsonSerializerOptions"/></returns>
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeOffsetJsonConverter());
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = false;
        return options;
    }

    /// <summary>
    /// 创建默认的 <see cref="IJsonSerializer"/> 实例
    /// </summary>
    /// <returns>JSON 序列化器实例</returns>
    public static IJsonSerializer Create()
    {
        var options = CreateDefaultOptions();

        // comments by lewis at 20230714
        // 不应该使用这个功能， 如果一个字典的键值中， 同时有 a 和 A， 会导致有两个 a 在序列化结果中
        // options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        return new TextJsonSerializer(options);
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    /// <param name="obj">待序列化的对象</param>
    /// <returns>JSON 字符串</returns>
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// 将对象序列化为 UTF-8 字节数组
    /// </summary>
    /// <param name="obj">待序列化的对象</param>
    /// <returns>UTF-8 字节数组</returns>
    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, options);
    }

    /// <summary>
    /// 将值序列化并写入流
    /// </summary>
    /// <param name="utf8Json">目标流</param>
    /// <param name="value">待序列化的值</param>
    /// <typeparam name="TValue">值类型</typeparam>
    public void Serialize<TValue>(Stream utf8Json, TValue value)
    {
        JsonSerializer.Serialize(utf8Json, value, options);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// 从流中反序列化为指定类型
    /// </summary>
    /// <param name="json">包含 JSON 数据的流</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    public T Deserialize<T>(Stream json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型（运行时确定类型）
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="returnType">目标类型</param>
    /// <returns>反序列化后的对象</returns>
    public object Deserialize(string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, options);
    }
}
