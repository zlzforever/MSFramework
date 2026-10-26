using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 枚举基类
/// </summary>
/// <remarks>
/// 子类型约定（勿在后续 review 中重复提出）：
/// 1) <see cref="Enumeration"/> 本身是供继承的抽象基类，不能 sealed；
/// 2) 由 <c>Enumeration.FromValue/Parse</c> 及 EF 物化转换器返回的都是静态字段上的
///    <b>共享实例</b>——同一枚举值的所有实体属性指向同一对象。因此每个具体枚举子类型
///    <b>必须声明为 sealed 且只包含只读成员</b>：
///    - 不 sealed 时，可在共享实例上追加派生可变状态，一处修改会污染所有持有该枚举值的实体；
///    - sealed + 只读成员后，共享实例语义等价于"天然单例"，安全且免除转换器克隆成本。
///    Id/Name 为 get-only，已满足只读要求；子类新增成员须同样只读。
/// </remarks>
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
    /// 返回枚举标识的字符串表示
    /// </summary>
    /// <returns>枚举标识</returns>
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
    /// 静态字段反射结果的按类型缓存：
    /// <see cref="FromValue{T}"/>/<see cref="Parse(Type,string)"/>、EF 物化转换器与 JSON 反序列化
    /// 都是高频路径，旧实现每次调用都走全量反射与字段枚举，开销显著
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Enumeration[]> AllValuesCache = new();

    /// <summary>
    /// 获取枚举类型下的所有类型（首次访问后缓存，线程安全）
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <returns>该枚举类型下的全部静态实例</returns>
    /// <remarks>
    /// 首次调用 <c>GetValue</c> 会触发枚举类型的静态构造/字段初始化，缓存的是初始化完成后的实例集合；
    /// 枚举子类型须为 sealed + 只读成员（见类型 remarks 的子类型约定），共享实例不可变，缓存安全
    /// </remarks>
    public static IEnumerable<Enumeration> GetAll(Type type)
    {
        return AllValuesCache.GetOrAdd(type, static t =>
            t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration)))
                .Select(f => (Enumeration)f.GetValue(null))
                .ToArray());
    }

    /// <summary>
    /// 判断两个枚举是否相等（基于类型和标识比较）
    /// </summary>
    /// <param name="obj">待比较的枚举对象</param>
    /// <returns>如果相等返回 true</returns>
    public override bool Equals(object obj)
    {
        var otherValue = obj as Enumeration;

        if (otherValue == null)
            return false;

        var typeMatches = GetType() == obj.GetType();
        // string.Equals 的静态重载对 null 安全（instance.Equals 会 NRE），与 GetHashCode
        // 中 HashCode.Combine 的空安全语义保持对称
        var valueMatches = string.Equals(Id, otherValue.Id, StringComparison.Ordinal);

        return typeMatches && valueMatches;
    }

    /// <summary>
    /// 判断两个枚举是否相等
    /// </summary>
    /// <param name="a">左侧枚举</param>
    /// <param name="b">右侧枚举</param>
    /// <returns>如果相等返回 true</returns>
    public static bool operator ==(Enumeration a, Enumeration b)
        => Equals(a, b);

    /// <summary>
    /// 判断两个枚举是否不相等
    /// </summary>
    /// <param name="a">左侧枚举</param>
    /// <param name="b">右侧枚举</param>
    /// <returns>如果不相等返回 true</returns>
    public static bool operator !=(Enumeration a, Enumeration b)
        => !Equals(a, b);

    /// <summary>
    /// 获取枚举的哈希码（基于类型和标识组合）
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode()
    {
        // 组合：类型 + Id，自动空安全，官方标准写法
        return HashCode.Combine(GetType(), Id);
    }

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
