using System;

namespace Ordering.Domain.AggregateRoots.CompositeKey;

/// <summary>
/// 订单项复合主键值对象（方案 A：以值对象作为主键类型）。
/// <para>
/// 不可变 record 自动实现 <see cref="IEquatable{T}"/>，满足
/// <c>EntityBase&lt;TKey&gt;</c> 的 <c>TKey : IEquatable&lt;TKey&gt;</c> 约束；
/// 成员均为标量类型，可被 EF 的 <see cref="ValueConverter{TModel,TProvider}"/> 映射为单列主键。
/// </para>
/// </summary>
/// <param name="OrderId">订单标识（复合主键成员 1）</param>
/// <param name="ProductId">产品标识（复合主键成员 2）</param>
public sealed record OrderItemKey(string OrderId, string ProductId)
{
    /// <summary>
    /// 以分隔符拼接复合键成员，作为数据库存储字符串（与 <see cref="Parse"/> 互逆）
    /// </summary>
    /// <returns>形如 "orderId|productId" 的存储字符串</returns>
    public override string ToString() => $"{OrderId}|{ProductId}";

    /// <summary>
    /// 从数据库存储字符串还原复合主键值对象（与 <see cref="ToString"/> 互逆）
    /// </summary>
    /// <param name="value">以 '|' 分隔的复合键存储字符串</param>
    /// <returns>还原后的复合主键值对象</returns>
    /// <exception cref="ArgumentException">存储字符串不是合法的两段复合键时抛出</exception>
    public static OrderItemKey Parse(string value)
    {
        var parts = value.Split('|');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new ArgumentException($"非法的复合主键存储字符串: {value}", nameof(value));
        }

        return new OrderItemKey(parts[0], parts[1]);
    }
}
