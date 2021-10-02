using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain
{
	public abstract class ModificationAggregateRoot : ModificationAggregateRoot<ObjectId>
	{
		protected ModificationAggregateRoot(ObjectId id) : base(id)
		{
		}
	}

	public abstract class ModificationAggregateRoot<TKey> : CreationAggregateRoot<TKey>,
		IModification where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		public string LastModifierId { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		public DateTimeOffset? LastModificationTime { get; private set; }

		public virtual void SetModification(string userId,
			DateTimeOffset lastModificationTime = default)
		{
			LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;

			if (!string.IsNullOrWhiteSpace(userId))
			{
				LastModifierId = userId;
			}
		}

		protected ModificationAggregateRoot(TKey id) : base(id)
		{
		}
	}
}