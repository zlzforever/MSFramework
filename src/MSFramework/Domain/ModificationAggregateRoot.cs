using System;
using MSFramework.Shared;

namespace MSFramework.Domain
{
	public abstract class ModificationAggregateRoot : ModificationAggregateRoot<ObjectId>
	{
		protected ModificationAggregateRoot(ObjectId id) : base(id)
		{
		}
	}

	public abstract class ModificationAggregateRoot<TKey> : CreationAggregateRoot<TKey>,
		IModification
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		public string ModificationUserId { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		public string ModificationUserName { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		public DateTimeOffset? ModificationTime { get; private set; }

		public virtual void SetModification(string userId, string userName,
			DateTimeOffset lastModificationTime = default)
		{
			ModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;

			if (!string.IsNullOrWhiteSpace(userId))
			{
				ModificationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName))
			{
				ModificationUserName = userName;
			}
		}

		protected ModificationAggregateRoot(TKey id) : base(id)
		{
		}
	}
}