using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Collections.Generic;

/// <summary>
/// Enumerable集合扩展方法
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// 将集合展开并分别转换成字符串，再以指定的分隔符衔接，拼成一个字符串返回。默认分隔符为逗号
    /// </summary>
    /// <param name="collection"> 要处理的集合 </param>
    /// <param name="separator"> 分隔符，默认为逗号 </param>
    /// <returns> 拼接后的字符串 </returns>
    public static string JoinString<T>(this IEnumerable<T> collection, string separator)
    {
        return string.Join(separator, collection);
    }

    public static string JoinString<T, TProperty>(this IEnumerable<T> collection,
        string separator, Func<T, TProperty> selector)
    {
        return string.Join(separator, collection.Select(selector));
    }

    // /// <summary>
    // /// 集合是否为空
    // /// </summary>
    // /// <param name="collection"> 要处理的集合 </param>
    // /// <typeparam name="T"> 动态类型 </typeparam>
    // /// <returns> 为空返回True，不为空返回False </returns>
    // public static bool IsEmpty<T>(this IEnumerable<T> collection)
    // {
    //     return collection == null || !collection.Any();
    // }

    // /// <summary>
    // /// 根据指定条件返回集合中不重复的元素
    // /// </summary>
    // /// <typeparam name="T">动态类型</typeparam>
    // /// <typeparam name="TKey">动态筛选条件类型</typeparam>
    // /// <param name="source">要操作的源</param>
    // /// <param name="keySelector">重复数据筛选条件</param>
    // /// <returns>不重复元素的集合</returns>
    // public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    // {
    //     Check.NotNull(keySelector, nameof(keySelector));
    //     return source.GroupBy(keySelector).Select(group => group.First());
    // }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        if (list == null)
        {
            return true;
        }

        return !list.Any();
    }

    public static bool HasDuplicates<T, TProp>(this IEnumerable<T> list, Func<T, TProp> selector)
    {
        var d = new HashSet<TProp>();
        return list.Any(t => !d.Add(selector(t)));
    }
}
