using System.Collections.Generic;

namespace MSFramework.Extensions
{
	/// <summary>
	/// 字典辅助扩展方法
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
		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary.TryGetValue(key, out var value) ? value : default;
		}

		/// <summary>
		/// 获取指定键的值，不存在则按指定委托添加值
		/// </summary>
		/// <typeparam name="TKey">字典键类型</typeparam>
		/// <typeparam name="TValue">字典值类型</typeparam>
		/// <param name="dictionary">要操作的字典</param>
		/// <param name="key">指定键名</param>
		/// <param name="value">添加值</param>
		/// <returns>获取到的值</returns>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out var v))
			{
				return v;
			}

			return dictionary[key] = value;
		}
	}
}