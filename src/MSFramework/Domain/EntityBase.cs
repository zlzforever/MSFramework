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
    /// 判断主键是否等于指定值
    /// </summary>
    /// <param name="other">要比较的主键值</param>
    /// <returns>如果相等返回 true</returns>
    public bool Equals(TKey other)
    {
        return !Equals(null, other) && Equals(Id, other);
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

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// 获取实体的哈希码（基于主键）
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode()
    {
        return Id == null ? 0 : Id.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}
