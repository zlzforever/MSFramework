using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 枚举基类
/// </summary>
public abstract class Enumeration(string id, string name)
{
    /// <summary>
    /// 标识
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Id;

    /// <summary>
    /// 获取枚举类型下的所有类型
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var enumerations = GetAll(typeof(T));
        return enumerations.Cast<T>();
    }

    /// <summary>
    /// 获取枚举类型下的所有类型
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <returns></returns>
    public static IEnumerable<Enumeration> GetAll(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration))).Select(f => (Enumeration)f.GetValue(null));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        var otherValue = obj as Enumeration;

        if (otherValue == null)
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator ==(Enumeration a, Enumeration b)
        => Equals(a, b);

    /// <summary>
    ///
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(Enumeration a, Enumeration b)
        => !Equals(a, b);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => Id.GetHashCode();


    /// <summary>
    /// 字符串转换为枚举类型
    /// </summary>
    /// <param name="value">字符串</param>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <returns>枚举值</returns>
    public static T FromValue<T>(string value) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    /// <summary>
    /// 字符串转换为枚举类型
    /// </summary>
    /// <param name="displayName">字符串</param>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <returns>枚举值</returns>
    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);
        return matchingItem;
    }

    /// <summary>
    /// 字符串转换为枚举类型
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="value">字符串</param>
    /// <returns>枚举值</returns>
    public static Enumeration Parse(Type type, string value)
    {
        var matchingItem = GetAll(type).FirstOrDefault(x => x.Id == value);

        if (matchingItem == null)
        {
            throw new InvalidOperationException($"字符串 '{value}' 不是枚举类型 {type}");
        }

        return matchingItem;
    }

    // public int CompareTo(object other)
    // {
    //     if (other == null)
    //     {
    //         return -1;
    //     }
    //
    //     return string.Compare(Id, ((Enumeration)other).Id, StringComparison.Ordinal);
    // }

    private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

        return matchingItem;
    }
}
