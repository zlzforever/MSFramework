using System;
using System.ComponentModel;
using System.Reflection;

namespace MSFramework.Domain.Entity
{
	public abstract class EntityBase : IEntity
	{
	}

	/// <summary>
	/// 实体类基类
	/// </summary>
	[Serializable]
	public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey> where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 
		/// </summary>
		[Description("唯一标识")]
		public TKey Id { get; private set; }

		public bool IsTransient()
		{
			return Id.Equals(default);
		}

		protected EntityBase()
		{
		}

		protected EntityBase(TKey id)
		{
			Id = id;
		}

		/// <summary>
		/// 判断两个实体是否是同一数据记录的实体
		/// </summary>
		/// <param name="obj">要比较的实体信息</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (!(obj is EntityBase<TKey>))
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

			return Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id == null ? 0 : Id.GetHashCode();
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

		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}";
		}
	}
}