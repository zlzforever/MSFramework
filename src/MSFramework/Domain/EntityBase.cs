using System;

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
	public abstract class EntityBase<TKey> : EntityBase, IEntity<TKey>, IComparable<EntityBase<TKey>>
	{
		private int? _hashCodeCache;
		private TKey _id;

		/// <inheritdoc/>
		public TKey Id
		{
			get => _id;
			protected set => _id = value;
		}

		protected EntityBase() : this(default)
		{
		}

		protected EntityBase(TKey id)
		{
			Id = id;
		}

		public bool IsTransient()
		{
			return EntityHelper.HasDefaultId(this);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is EntityBase<TKey>))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (GetType() != obj.GetType())
			{
				return false;
			}

			var item = (EntityBase<TKey>) obj;

			if (item.IsTransient() || IsTransient())
			{
				return false;
			}
			else
			{
				return item.Id.Equals(Id);
			}
		}

		public override int GetHashCode() => ComputeHashCode();

		public override object[] GetKeys()
		{
			return new object[] {Id};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}";
		}

		private int ComputeHashCode()
		{
			if (!IsTransient())
			{
				_hashCodeCache ??= Id.GetHashCode() ^ 31;
				return _hashCodeCache.Value;
			}
			else
			{
				return base.GetHashCode();
			}
		}

		public int CompareTo(EntityBase<TKey> other)
		{
			throw new NotImplementedException();
		}
	}
}