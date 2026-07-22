using System;
using System.IO;
using System.Text;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     基于 Newtonsoft.Json 的 JSON 序列化器实现，支持 ObjectId 和 Enumeration 类型转换
/// </summary>
public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings _settings;

    /// <summary>
    ///     初始化序列化器，若未提供设置则创建默认配置（含 ObjectId、Enumeration 转换器和合成解析器）
    /// </summary>
    /// <param name="settings">JSON 序列化设置，为 null 时使用默认配置</param>
    public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
    {
        if (settings == null)
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new ObjectIdConverter());
            settings.Converters.Add(new EnumerationConverter());
            settings.ContractResolver = new CompositeContractResolver
            {
                new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
            };
        }

        _settings = settings;
    }

    /// <summary>
    ///     将对象序列化为 JSON 字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>JSON 字符串</returns>
    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    /// <summary>
    ///     将对象序列化为 UTF-8 编码的字节数组
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>UTF-8 字节数组</returns>
    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
    }

    /// <summary>
    ///     将值序列化到流中（暂未实现）
    /// </summary>
    /// <param name="utf8Json">输出流</param>
    /// <param name="value">要序列化的值</param>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <exception cref="NotImplementedException">该方法尚未实现</exception>
    public void Serialize<TValue>(Stream utf8Json, TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     将 JSON 字符串反序列化为指定类型的对象
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    /// <summary>
    ///     从流中读取 JSON 并反序列化为指定类型的对象
    /// </summary>
    /// <param name="json">包含 JSON 的流</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    public T Deserialize<T>(Stream json)
    {
        using var reader = new StreamReader(json);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), _settings);
    }

    /// <summary>
    ///     将 JSON 字符串反序列化为指定运行时类型的对象
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="type">目标运行时类型</param>
    /// <returns>反序列化后的对象</returns>
    public object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type, _settings);
    }
}
