using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using MediatR;

namespace MSFramework.Domain.Entity
{
	/// <summary>
	/// 实体类基类
	/// </summary>
	[Serializable]
	public abstract class EntityBase<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
	{
		/// <inheritdoc/>
		public virtual TKey Id { get; set; }

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

		public virtual bool IsTransient()
		{
			if (EqualityComparer<TKey>.Default.Equals(Id, default))
			{
				return true;
			}

			//Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
			if (typeof(TKey) == typeof(int))
			{
				return Convert.ToInt32(Id) <= 0;
			}

			if (typeof(TKey) == typeof(long))
			{
				return Convert.ToInt64(Id) <= 0;
			}

			return false;
		}

		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}";
		}
	}
}