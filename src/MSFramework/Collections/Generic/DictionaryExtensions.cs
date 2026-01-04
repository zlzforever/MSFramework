using System;
using System.Collections.Generic;

namespace MicroserviceFramework.Collections.Generic;

/// <summary>
/// 字典辅助扩展方法
/// 只对 Dictionary 进行扩展，ConcurrentDictionary 或其它字典实现可能不一样
/// </summary>
public static class DictionaryExtensions
{
    /// <param name="dictionary">要操作的字典</param>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        /// <summary>
        /// 获取指定键的值，不存在则添加值
        /// </summary>
        /// <param name="key">指定键名</param>
        /// <param name="value">添加值</param>
        /// <returns>获取到的值</returns>
        public TValue GetOrAdd(TKey key, TValue value)
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
        /// <param name="key">键</param>
        /// <param name="factory">值工厂</param>
        public void AddOrUpdate(TKey key, Func<TValue> factory)
        {
            dictionary[key] = factory();
        }

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            dictionary[key] = value;
        }
    }
}
