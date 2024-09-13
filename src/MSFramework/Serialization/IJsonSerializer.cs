using System;
using System.IO;

namespace MicroserviceFramework.Serialization;

/// <summary>
///
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    string Serialize(object obj);

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    byte[] SerializeToUtf8Bytes(object obj);

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(string json);

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(Stream json);

    /// <summary>
    ///
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    object Deserialize(string json, Type type);
}
