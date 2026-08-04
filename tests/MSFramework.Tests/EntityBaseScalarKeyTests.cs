using System;
using MicroserviceFramework.Domain;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 标量主键场景下实体相等性与瞬态判断测试。
/// 覆盖 TKey 标量白名单（string / int / Guid）下 <see cref="EntityBase{TKey}"/> 的
/// Equals / GetHashCode / 瞬态（Transient）判断行为，替代已移除的复合值对象键（方案 A）用例。
/// </summary>
public class EntityBaseScalarKeyTests
{
    /// <summary>
    /// 使用 string 主键的实体
    /// </summary>
    public class StringIdEntity : EntityBase<string>
    {
        public StringIdEntity(string id) : base(id)
        {
        }
    }

    /// <summary>
    /// 使用 int 主键的实体
    /// </summary>
    public class IntIdEntity : EntityBase<int>
    {
        public IntIdEntity(int id) : base(id)
        {
        }
    }

    /// <summary>
    /// 使用 Guid 主键的实体
    /// </summary>
    public class GuidIdEntity : EntityBase<Guid>
    {
        public GuidIdEntity(Guid id) : base(id)
        {
        }
    }

    /// <summary>
    /// 不同实体类型，但具有相同 string 主键（用于验证类型 IS-A 校验）
    /// </summary>
    public class AnotherStringIdEntity : EntityBase<string>
    {
        public AnotherStringIdEntity(string id) : base(id)
        {
        }
    }

    [Fact]
    public void StringKey_Equal_WhenSameValue()
    {
        var entity1 = new StringIdEntity("A1");
        var entity2 = new StringIdEntity("A1");

        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1 == entity2);
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void StringKey_NotEqual_WhenValueDiffers()
    {
        var entity1 = new StringIdEntity("A1");
        var entity2 = new StringIdEntity("A2");

        Assert.False(entity1.Equals(entity2));
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void StringKey_NotEqual_ForDifferentTypes()
    {
        var entity = new StringIdEntity("A1");
        var another = new AnotherStringIdEntity("A1");

        Assert.False(entity.Equals(another));
    }

    [Fact]
    public void StringKey_NullKey_IsTransient()
    {
        var entity1 = new StringIdEntity(null);
        var entity2 = new StringIdEntity(null);

        // 两个瞬态实体（主键为 default）即使键相同也不相等
        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1 == entity2);
    }

    [Fact]
    public void IntKey_Equal_WhenSameValue()
    {
        var entity1 = new IntIdEntity(42);
        var entity2 = new IntIdEntity(42);

        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1 == entity2);
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void IntKey_NotEqual_WhenValueDiffers()
    {
        var entity1 = new IntIdEntity(42);
        var entity2 = new IntIdEntity(43);

        Assert.False(entity1.Equals(entity2));
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void IntKey_DefaultKey_IsTransient()
    {
        // default(int) 即 0，应视为瞬态
        var entity1 = new IntIdEntity(default);
        var entity2 = new IntIdEntity(default);

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1 == entity2);
    }

    [Fact]
    public void GuidKey_Equal_WhenSameValue()
    {
        var id = Guid.NewGuid();
        var entity1 = new GuidIdEntity(id);
        var entity2 = new GuidIdEntity(id);

        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1 == entity2);
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void GuidKey_DefaultKey_IsTransient()
    {
        // default(Guid) 即 Guid.Empty，应视为瞬态
        var entity1 = new GuidIdEntity(default);
        var entity2 = new GuidIdEntity(default);

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1 == entity2);
    }
}
