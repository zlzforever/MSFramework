using System;
using System.Collections.Generic;
using MicroserviceFramework.Collections.Generic;

namespace MicroserviceFramework.Utils;

/// <summary>
/// 参数合法性检查类
/// </summary>
public static class Check
{
    /// <summary>
    /// 检查参数不为 null，否则抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <param name="value">待检查的参数值</param>
    /// <param name="parameterName">参数名称</param>
    /// <typeparam name="T">参数类型</typeparam>
    /// <returns>参数值（不为 null）</returns>
    /// <exception cref="ArgumentNullException">参数值为 null 时抛出</exception>
    public static T NotNull<T>(T value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return value;
    }

    /// <summary>
    /// 检查参数不为 null，否则抛出带自定义消息的 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <param name="value">待检查的参数值</param>
    /// <param name="parameterName">参数名称</param>
    /// <param name="message">自定义错误消息</param>
    /// <typeparam name="T">参数类型</typeparam>
    /// <returns>参数值（不为 null）</returns>
    /// <exception cref="ArgumentNullException">参数值为 null 时抛出</exception>
    public static T NotNull<T>(T value, string parameterName, string message)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName, message);
        }

        return value;
    }

    /// <summary>
    /// 检查字符串不为 null 或空，否则抛出 <see cref="ArgumentException"/>
    /// </summary>
    /// <param name="value">待检查的字符串</param>
    /// <param name="parameterName">参数名称</param>
    /// <returns>字符串值（不为 null 或空）</returns>
    /// <exception cref="ArgumentException">字符串为 null 或空时抛出</exception>
    public static string NotNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{parameterName} can not be null or empty!", parameterName);
        }

        return value;
    }

    /// <summary>
    /// 检查集合不为 null 或空，否则抛出 <see cref="ArgumentException"/>
    /// </summary>
    /// <param name="value">待检查的集合</param>
    /// <param name="parameterName">参数名称</param>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <returns>集合值（不为 null 或空）</returns>
    /// <exception cref="ArgumentException">集合为 null 或空时抛出</exception>
    public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, string parameterName)
    {
        if (value.IsNullOrEmpty())
        {
            throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
        }

        return value;
    }
}
