using MSFramework.Domain.Event;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MSFramework.Domain.Entity
{
	/// <summary>
	/// 实体类基类
	/// </summary>
	[Serializable]
	public abstract class EntityBase<TKey> : IEntity<TKey>
	{
		/// <inheritdoc/>
		public virtual TKey Id { get; set; }
		private ICollection<IDomainEvent> _domainEvents;

		protected EntityBase()
		{

		}


		/// <summary>
		/// 判断两个实体是否是同一数据记录的实体
		/// </summary>
		/// <param name="obj">要比较的实体信息</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (!(obj is EntityBase<TKey> entity))
			{
				return false;
			}

			return IsKeyEqual(entity.Id, Id);
		}

		/// <summary>
		/// 实体ID是否相等
		/// </summary>
		public static bool IsKeyEqual(TKey id1, TKey id2)
		{
			if (id1 == null && id2 == null)
			{
				return true;
			}

			if (id1 == null || id2 == null)
			{
				return false;
			}

			return Equals(id1, id2);
		}

		public override int GetHashCode()
		{
			if (Id == null)
			{
				return 0;
			}

			return Id.GetHashCode();
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
			return Id == default;
		}

		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}";
		}

		protected virtual void AddDomainEvent(IDomainEvent eventItem)
		{
			_domainEvents = _domainEvents ?? new Collection<IDomainEvent>();
			_domainEvents.Add(eventItem);
		}

		protected virtual void RemoveDomainEvent(IDomainEvent eventItem)
		{
			if (_domainEvents.Contains(eventItem))
			{
				_domainEvents.Remove(eventItem);
			}
		}

		protected virtual void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}

		public virtual IEnumerable<IDomainEvent> GetDomainEvents()
		{
			return _domainEvents;
		}
	}
}
