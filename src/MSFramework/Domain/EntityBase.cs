using System;
using System.Reflection;

namespace MSFramework.Domain
{
	/// <inheritdoc/>
	[Serializable]
	public abstract class EntityBase : IEntity
	{
		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Keys = {string.Join(", ", GetKeys())}";
		}

		public abstract object[] GetKeys();
	}

	/// <inheritdoc cref="IEntity{TKey}" />
	[Serializable]
	public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey>
	{
		/// <inheritdoc/>
		public TKey Id { get; protected set; }
		
		protected EntityBase(TKey id)
		{
			Id = id;
		}

		public bool EntityEquals(object obj)
		{
			if (obj == null || !(obj is EntityBase<TKey>))
			{
				return false;
			}

			//Same instances must be considered as equal
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			//Transient objects are not considered as equal
			var other = (EntityBase<TKey>) obj;
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

			// todo:
			// //Different tenants may have an entity with same Id.
			// if (this is IMultiTenant && other is IMultiTenant &&
			//     this.As<IMultiTenant>().TenantId != other.As<IMultiTenant>().TenantId)
			// {
			// 	return false;
			// }

			return Id.Equals(other.Id);
		}

		public override object[] GetKeys()
		{
			return new object[] {Id};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}";
		}
	}
}