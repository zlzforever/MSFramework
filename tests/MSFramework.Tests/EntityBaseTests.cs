using MicroserviceFramework.Domain;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 复合主键（值对象键）场景下实体相等性与瞬态判断测试。
/// 覆盖 T2：验证 EntityBase{TKey} 在 record / record struct 复合键下的
/// Equals / GetHashCode / 瞬态（Transient）判断行为。
/// </summary>
public class EntityBaseTests
{
    /// <summary>
    /// 复合主键值对象（record class，引用类型）
    /// </summary>
    public sealed record OrderItemKey(string OrderId, string ProductId);

    /// <summary>
    /// 复合主键值对象（record struct，值类型）
    /// </summary>
    public readonly record struct OrderItemStructKey(string OrderId, string ProductId);

    /// <summary>
    /// 使用 record class 复合键的实体
    /// </summary>
    public class OrderItem : EntityBase<OrderItemKey>
    {
        public OrderItem(OrderItemKey id) : base(id)
        {
        }
    }

    /// <summary>
    /// 使用 record struct 复合键的实体
    /// </summary>
    public class OrderItemStruct : EntityBase<OrderItemStructKey>
    {
        public OrderItemStruct(OrderItemStructKey id) : base(id)
        {
        }
    }

    /// <summary>
    /// 不同实体类型，但具有相同复合键（用于验证类型 IS-A 校验）
    /// </summary>
    public class AnotherOrderItem : EntityBase<OrderItemKey>
    {
        public AnotherOrderItem(OrderItemKey id) : base(id)
        {
        }
    }

    [Fact]
    public void RecordClass_CompositeKey_Equal()
    {
        var id1 = new OrderItemKey("O1", "P1");
        var id2 = new OrderItemKey("O1", "P1");
        var order1 = new OrderItem(id1);
        var order2 = new OrderItem(id2);

        Assert.True(order1.Equals(order2));
        Assert.True(order1 == order2);
        Assert.Equal(order1.GetHashCode(), order2.GetHashCode());
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void RecordClass_CompositeKey_NotEqual_WhenAnyMemberDiffers()
    {
        var order1 = new OrderItem(new OrderItemKey("O1", "P1"));
        var order2 = new OrderItem(new OrderItemKey("O2", "P1"));
        var order3 = new OrderItem(new OrderItemKey("O1", "P2"));

        Assert.False(order1.Equals(order2));
        Assert.False(order1.Equals(order3));
        Assert.True(order1 != order2);
    }

    [Fact]
    public void RecordClass_CompositeKey_NotEqual_ForDifferentTypes()
    {
        var id = new OrderItemKey("O1", "P1");
        var order = new OrderItem(id);
        var another = new AnotherOrderItem(id);

        Assert.False(order.Equals(another));
    }

    [Fact]
    public void RecordClass_NullKey_IsTransient()
    {
        var order1 = new OrderItem(null!);
        var order2 = new OrderItem(null!);

        // 两个瞬态实体（主键为 default）即使键相同也不相等
        Assert.False(order1.Equals(order2));
        Assert.False(order1 == order2);
    }

    [Fact]
    public void RecordStruct_CompositeKey_Equal()
    {
        var order1 = new OrderItemStruct(new OrderItemStructKey("O1", "P1"));
        var order2 = new OrderItemStruct(new OrderItemStructKey("O1", "P1"));

        Assert.True(order1.Equals(order2));
        Assert.True(order1 == order2);
        Assert.Equal(order1.GetHashCode(), order2.GetHashCode());
    }

    [Fact]
    public void RecordStruct_CompositeKey_NotEqual_WhenAnyMemberDiffers()
    {
        var order1 = new OrderItemStruct(new OrderItemStructKey("O1", "P1"));
        var order2 = new OrderItemStruct(new OrderItemStructKey("O1", "P2"));

        Assert.False(order1.Equals(order2));
        Assert.True(order1 != order2);
    }

    [Fact]
    public void RecordStruct_DefaultKey_IsTransient()
    {
        // default(OrderItemStructKey) 即所有成员为 null / default
        var order1 = new OrderItemStruct(default);
        var order2 = new OrderItemStruct(default);

        Assert.False(order1.Equals(order2));
        Assert.False(order1 == order2);
    }

    [Fact]
    public void RecordStruct_EmptyStringKey_IsNotTransient()
    {
        // 空字符串键不等于 default（null），应视为非瞬态
        var order1 = new OrderItemStruct(new OrderItemStructKey("", ""));
        var order2 = new OrderItemStruct(new OrderItemStructKey("", ""));

        Assert.True(order1.Equals(order2));
        Assert.Equal(order1.GetHashCode(), order2.GetHashCode());
    }
}
