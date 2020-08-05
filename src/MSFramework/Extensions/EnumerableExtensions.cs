using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSFramework.Shared;

namespace MSFramework.Extensions
{
	/// <summary>
	/// Enumerable集合扩展方法
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// 打乱一个集合的项顺序
		/// </summary>
		public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			return source.OrderBy(m => Guid.NewGuid());
		}

		/// <summary>
		/// 将集合展开并分别转换成字符串，再以指定的分隔符衔接，拼成一个字符串返回。默认分隔符为逗号
		/// </summary>
		/// <param name="collection"> 要处理的集合 </param>
		/// <param name="separator"> 分隔符，默认为逗号 </param>
		/// <returns> 拼接后的字符串 </returns>
		public static string ExpandAndToString<T>(this IEnumerable<T> collection, string separator = ",")
		{
			return collection.ExpandAndToString(t => t?.ToString(), separator);
		}

		/// <summary>
		/// 循环集合的每一项，调用委托生成字符串，返回合并后的字符串。默认分隔符为逗号
		/// </summary>
		/// <param name="collection">待处理的集合</param>
		/// <param name="itemFormatFunc">单个集合项的转换委托</param>
		/// <param name="separator">分隔符，默认为逗号</param>
		/// <typeparam name="T">泛型类型</typeparam>
		/// <returns></returns>
		public static string ExpandAndToString<T>(this IEnumerable<T> collection, Func<T, string> itemFormatFunc,
			string separator = ",")
		{
			Check.NotNull(itemFormatFunc, nameof(itemFormatFunc));

			collection = collection as IList<T> ?? collection.ToList();
			if (!collection.Any())
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			var i = 0;
			var count = collection.Count();
			foreach (var t in collection)
			{
				if (i == count - 1)
				{
					sb.Append(itemFormatFunc(t));
				}
				else
				{
					sb.Append(itemFormatFunc(t) + separator);
				}

				i++;
			}

			return sb.ToString();
		}

		/// <summary>
		/// 集合是否为空
		/// </summary>
		/// <param name="collection"> 要处理的集合 </param>
		/// <typeparam name="T"> 动态类型 </typeparam>
		/// <returns> 为空返回True，不为空返回False </returns>
		public static bool IsEmpty<T>(this IEnumerable<T> collection)
		{
			return !collection?.Any() ?? true;
		}

		/// <summary>
		/// 根据指定条件返回集合中不重复的元素
		/// </summary>
		/// <typeparam name="T">动态类型</typeparam>
		/// <typeparam name="TKey">动态筛选条件类型</typeparam>
		/// <param name="source">要操作的源</param>
		/// <param name="keySelector">重复数据筛选条件</param>
		/// <returns>不重复元素的集合</returns>
		public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			Check.NotNull(keySelector, nameof(keySelector));
			return source.GroupBy(keySelector).Select(group => group.First());
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
		{
			if (list == null)
			{
				return true;
			}

			if (!list.Any())
			{
				return true;
			}

			return false;
		}

		public static bool HasDuplicates<T, TProp>(this IEnumerable<T> list, Func<T, TProp> selector)
		{
			var d = new HashSet<TProp>();
			foreach (var t in list)
			{
				if (!d.Add(selector(t)))
				{
					return true;
				}
			}

			return false;
		}
	}
}