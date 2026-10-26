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
    ///     缓存的 <c>JsonSerializer.IsCheckAdditionalContentSet</c>（internal 成员）查询委托：
    ///     用于查询"调用方是否<b>显式</b>设置过 CheckAdditionalContent"（三态语义，公开 API 不暴露）。
    ///     未来版本若移除该成员，回退为"默认开启检查"
    /// </summary>
    private static readonly Func<JsonSerializer, bool> IsCheckAdditionalContentSetQuery =
        BuildCheckAdditionalContentSetQuery();

    /// <summary>
    ///     编译 internal 成员 <c>IsCheckAdditionalContentSet</c> 的查询委托；成员不存在时返回恒 false
    ///     （即视为"未显式设置"，后续按框架契约默认开启额外内容检查）
    /// </summary>
    /// <returns>查询委托（serializer → 是否显式设置）</returns>
    private static Func<JsonSerializer, bool> BuildCheckAdditionalContentSetQuery()
    {
        try
        {
            var method = typeof(JsonSerializer).GetMethod("IsCheckAdditionalContentSet",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null)
            {
                return _ => false;
            }

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(JsonSerializer));
            return System.Linq.Expressions.Expression
                .Lambda<Func<JsonSerializer, bool>>(
                    System.Linq.Expressions.Expression.Call(parameter, method), parameter)
                .Compile();
        }
        catch
        {
            // 反射失败：按"未显式设置"处理（默认开启检查）
            return _ => false;
        }
    }

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
    ///     将值序列化到流中（UTF-8 无 BOM）。
    ///     输出流所有权归调用方：writer 以 <c>leaveOpen: true</c> 创建，方法结束后流保持打开；
    ///     直接流式写出，不做整对象 JSON 字符串物化
    /// </summary>
    /// <param name="utf8Json">输出流，不允许为 null</param>
    /// <param name="value">要序列化的值</param>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <exception cref="ArgumentNullException">utf8Json 为 null 时抛出</exception>
    public void Serialize<TValue>(Stream utf8Json, TValue value)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);

        using var writer = new StreamWriter(
            utf8Json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        JsonSerializer.Create(_settings).Serialize(writer, value);
        writer.Flush();
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
    ///     从流中读取 JSON 并反序列化为指定类型的对象。
    ///     输入流所有权归调用方：reader 以 <c>leaveOpen: true</c> 创建，方法结束后流保持打开；
    ///     使用 <c>JsonTextReader</c> 流式解析，不做整流字符串物化。
    ///     与字符串重载共用同一 <c>CheckAdditionalContent</c> 策略：默认开启额外内容检查
    ///     （尾随对象/垃圾文本拒绝），但尊重 Newtonsoft 的完整语义——尾随注释允许、
    ///     调用方显式关闭检查时尊重配置
    /// </summary>
    /// <param name="json">包含 JSON 的流，不允许为 null</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    /// <exception cref="ArgumentNullException">json 为 null 时抛出</exception>
    /// <exception cref="global::Newtonsoft.Json.JsonReaderException">根对象之后存在额外内容
    /// （且 CheckAdditionalContent 开启）时抛出</exception>
    public T Deserialize<T>(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var reader = new JsonTextReader(new StreamReader(
            json, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true));

        // 与字符串重载（JsonConvert.DeserializeObject）复刻同一策略：
        // CreateDefault 不会开启 CheckAdditionalContent（三态默认"未设置"=False），
        // 未显式设置时按框架契约默认开启额外内容检查；显式设置（无论开/关）尊重调用方。
        // 检查本身由 Newtonsoft 完成（容忍尾随注释），不要手工读 token 模拟
        var serializer = JsonSerializer.CreateDefault(_settings);
        if (!IsCheckAdditionalContentSetQuery(serializer))
        {
            serializer.CheckAdditionalContent = true;
        }

        return serializer.Deserialize<T>(reader);
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
