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
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T NotNull<T>(T value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T NotNull<T>(T value, string parameterName, string message)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName, message);
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string NotNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{parameterName} can not be null or empty!", parameterName);
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, string parameterName)
    {
        if (value.IsNullOrEmpty())
        {
            throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
        }

        return value;
    }
}
