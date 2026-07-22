using System;
using System.IO;

namespace MicroserviceFramework.Serialization;

/// <summary>
/// JSON 序列化器抽象接口，定义对象的序列化和反序列化操作
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    /// <param name="obj">待序列化的对象</param>
    /// <returns>JSON 字符串</returns>
    string Serialize(object obj);

    /// <summary>
    /// 将对象序列化为 UTF-8 字节数组
    /// </summary>
    /// <param name="obj">待序列化的对象</param>
    /// <returns>UTF-8 字节数组</returns>
    byte[] SerializeToUtf8Bytes(object obj);

    /// <summary>
    /// 将对象序列化并写入流
    /// </summary>
    /// <param name="utf8Json">目标流</param>
    /// <param name="value">待序列化的值</param>
    /// <typeparam name="TValue">值类型</typeparam>
    void Serialize<TValue>(Stream utf8Json, TValue value);

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    T Deserialize<T>(string json);

    /// <summary>
    /// 从流中反序列化为指定类型
    /// </summary>
    /// <param name="json">包含 JSON 数据的流</param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>反序列化后的对象</returns>
    T Deserialize<T>(Stream json);

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型（运行时确定类型）
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="type">目标类型</param>
    /// <returns>反序列化后的对象</returns>
    object Deserialize(string json, Type type);
}
