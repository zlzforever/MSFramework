using System;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain
{
	public abstract class ModificationEntity : ModificationEntity<ObjectId>
	{
		protected ModificationEntity(ObjectId id) : base(id)
		{
		}
	}

	public abstract class ModificationEntity<TKey> : CreationEntity<TKey>, IModification where TKey : IEquatable<TKey>
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

		protected ModificationEntity(TKey id) : base(id)
		{
		}
	}
}