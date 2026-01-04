using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Collections.Generic;

/// <summary>
/// Enumerable集合扩展方法
/// </summary>
public static class EnumerableExtensions
{
    /// <param name="enumerable"> 要处理的集合 </param>
    /// <typeparam name="T"></typeparam>
    extension<T>(IEnumerable<T> enumerable)
    {
        /// <summary>
        /// 将集合展开并分别转换成字符串，再以指定的分隔符衔接，拼成一个字符串返回。默认分隔符为逗号
        /// </summary>
        /// <param name="separator"> 分隔符，默认为逗号 </param>
        /// <returns> 拼接后的字符串 </returns>
        public string JoinSeparator(string separator)
        {
            return string.Join(separator, enumerable);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="selector"></param>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        public string JoinSeparator<TProperty>(string separator, Func<T, TProperty> selector)
        {
            return string.Join(separator, enumerable.Select(selector));
        }

        /// <summary>
        /// 集合是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsNullOrEmpty()
        {
            if (enumerable == null)
            {
                return true;
            }

            return !enumerable.Any();
        }

        /// <summary>
        /// 是否存在重复值
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TProp"></typeparam>
        /// <returns></returns>
        public bool HasDuplicates<TProp>(Func<T, TProp> selector)
        {
            var d = new HashSet<TProp>();
            foreach (var item in enumerable)
            {
                var key = selector(item);
                if (!d.Add(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
