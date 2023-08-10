using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Collections.Generic;

/// <summary>
/// 字典辅助扩展方法
/// 只对 Dictionary 进行扩展，ConcurrentDictionary 或其它字典实现可能不一样
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// 从字典中获取值，不存在则返回字典<typeparamref name="TValue"/>类型的默认值
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    /// <param name="dictionary">要操作的字典</param>
    /// <param name="key">指定键名</param>
    /// <returns>获取到的值</returns>
    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var value) ? value : default;
    }

    /// <summary>
    /// 获取指定键的值，不存在则添加值
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">指定键名</param>
    /// <param name="value">添加值</param>
    /// <returns>获取到的值</returns>
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.TryGetValue(key, out var v))
        {
            return v;
        }

        return dictionary[key] = value;
    }

    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="dictionary">字典</param>
    /// <param name="key">键</param>
    /// <param name="factory">值工厂</param>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
        Func<TValue> factory)
    {
        dictionary[key] = factory();
    }

    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="dictionary">字典</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
        TValue value)
    {
        dictionary[key] = value;
    }
}
