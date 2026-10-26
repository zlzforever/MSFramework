using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 实体基类
/// </summary>
[Serializable]
public abstract class EntityBase : IEntity
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// 添加领域事件
    /// </summary>
    /// <param name="event">领域事件</param>
    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    /// <summary>
    /// 删除领域事件
    /// </summary>
    /// <param name="event">领域事件</param>
    public void RemoveDomainEvent(DomainEvent @event)
    {
        _domainEvents.Remove(@event);
    }

    /// <summary>
    /// 获取所有领域事件
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// 清空领域事件
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// 实体基类
/// </summary>
/// <typeparam name="TKey">实体主键类型，仅限标量白名单：string / int / long / ObjectId / Guid。
/// 该约束通过 <c>TKey : IEquatable&lt;TKey&gt;</c> 结合文档/XML 注释约定落地（非编译器强制）；
/// 复合主键请改用非泛型 <see cref="EntityBase"/> 多列模式（方案 B）</typeparam>
[Serializable]
public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey> where TKey : IEquatable<TKey>
{
    private TKey _id;

    /// <summary>
    /// 主键
    /// </summary>
    public TKey Id
    {
        get => _id;
        protected set => _id = value;
    }

    /// <summary>
    /// 初始化实体基类
    /// </summary>
    /// <param name="id">实体主键</param>
    protected EntityBase(TKey id)
    {
        Id = id;
    }

    /// <summary>
    /// 判断两个实体是否相等
    /// </summary>
    /// <param name="left">左侧实体</param>
    /// <param name="right">右侧实体</param>
    /// <returns>如果相等返回 true</returns>
    public static bool operator ==(EntityBase<TKey> left, EntityBase<TKey> right)
    {
        if (Equals(left, null))
        {
            return Equals(right, null);
        }

        return left.Equals(right);
    }

    /// <summary>
    /// 判断两个实体是否不相等
    /// </summary>
    /// <param name="left">左侧实体</param>
    /// <param name="right">右侧实体</param>
    /// <returns>如果不相等返回 true</returns>
    public static bool operator !=(EntityBase<TKey> left, EntityBase<TKey> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 判断实体主键是否等于指定值。
    /// </summary>
    /// <remarks>
    /// 按主键比较请使用本方法；实体相等语义保留在 <see cref="Equals(object)"/> 与
    /// <see cref="operator =="/> 中。
    /// </remarks>
    /// <param name="other">要比较的主键值</param>
    /// <returns>主键非默认且与实体主键相等时返回 true</returns>
    public bool HasId(TKey other)
    {
        return !Equals(null, other) && Equals(Id, other);
    }

    /// <summary>
    /// 判断实体主键是否等于指定值（历史重载）。
    /// </summary>
    /// <remarks>
    /// 保留该重载仅为源码兼容：旧代码中的 <c>entity.Equals(id)</c> 会绑定到本方法。
    /// 新代码请统一使用 <see cref="HasId(TKey)"/>；实体相等请使用 <see cref="Equals(object)"/>。
    /// </remarks>
    /// <param name="other">要比较的主键值</param>
    /// <returns>主键非默认且与实体主键相等时返回 true</returns>
    [Obsolete("请使用 HasId(TKey) 进行主键比较；实体相等请使用 Equals(object)。")]
    public bool Equals(TKey other)
    {
        return HasId(other);
    }

    /// <summary>
    /// 判断两个实体是否相等（基于主键和类型比较）
    /// </summary>
    /// <param name="obj">待比较的实体</param>
    /// <returns>如果相等返回 true</returns>
    public override bool Equals(object obj)
    {
        if (obj is not EntityBase<TKey> other)
        {
            return false;
        }

        //Same instances must be considered as equal
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        //Transient objects are not considered as equal
        if (EntityHelper.HasDefaultId(this) && EntityHelper.HasDefaultId(other))
        {
            return false;
        }

        //Must have a IS-A relation of types or must be same type
        var typeOfThis = GetType().GetTypeInfo();
        var typeOfOther = other.GetType().GetTypeInfo();
        if (!typeOfThis.IsAssignableFrom(typeOfOther) && !typeOfOther.IsAssignableFrom(typeOfThis))
        {
            return false;
        }

        //Different tenants may have an entity with same Id.
        // if (this is IMultiTenant && other is IMultiTenant &&
        //     this.As<IMultiTenant>().TenantId != other.As<IMultiTenant>().TenantId)
        // {
        // 	return false;
        // }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// 获取实体的哈希码（基于主键）。
    /// 经 <see cref="EqualityComparer{T}.Default"/> 计算以避免对值类型主键
    /// （int/long/Guid/ObjectId）的装箱分配：接口虚方法分派在 Comparer 类上完成，无装箱
    /// </summary>
    /// <returns>哈希码（主键为 null/默认值时返回 0）</returns>
    public override int GetHashCode()
    {
        return EqualityComparer<TKey>.Default.GetHashCode(Id);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}
