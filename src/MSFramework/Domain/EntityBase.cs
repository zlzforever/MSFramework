using System;
using System.Collections.Generic;
using System.Reflection;
using MicroserviceFramework.Collections.Generic;

namespace MicroserviceFramework.Domain
{
    /// <inheritdoc/>
    [Serializable]
    public abstract class EntityBase : IEntity
    {
        private readonly List<DomainEvent> _domainEvents;

        public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

        protected EntityBase()
        {
            _domainEvents = new List<DomainEvent>();
        }

        public void AddDomainEvent(DomainEvent @event)
        {
            _domainEvents.Add(@event);
        }

        public void RemoveDomainEvent(DomainEvent @event)
        {
            _domainEvents.Remove(@event);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[ENTITY: {GetType().Name}] Keys = {GetKeys().Join(", ")}";
        }

        public abstract object[] GetKeys();
    }

    /// <inheritdoc cref="IEntity{TKey}" />
    [Serializable]
    public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey> where TKey : IEquatable<TKey>
    {
        private TKey _id;

        /// <inheritdoc/>
        public TKey Id
        {
            get => _id;
            protected set => _id = value;
        }

        public override object[] GetKeys()
        {
            return new object[] { Id };
        }

        protected EntityBase(TKey id)
        {
            Id = id;
        }

        public static bool operator ==(EntityBase<TKey> left, EntityBase<TKey> right)
        {
            if (Equals(left, null))
            {
                return Equals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(EntityBase<TKey> left, EntityBase<TKey> right)
        {
            return !(left == right);
        }

        public bool Equals(TKey other)
        {
            return !Equals(null, other) && Equals(Id, other);
        }

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
}