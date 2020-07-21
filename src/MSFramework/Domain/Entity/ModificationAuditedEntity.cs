using System;

namespace MSFramework.Domain.Entity
{
	public abstract class ModificationAuditedEntity : ModificationAuditedEntity<Guid>
	{
		protected ModificationAuditedEntity(Guid id) : base(id)
		{
		}
	}

	public abstract class ModificationAuditedEntity<TKey> : CreationAuditedEntity<TKey>, IModificationAudited
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		public string LastModificationUserId { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		public string LastModificationUserName { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		public DateTimeOffset? LastModificationTime { get; private set; }

		public virtual void SetModificationAudited(string userId, string userName,
			DateTimeOffset lastModificationTime = default)
		{
			LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;

			if (!string.IsNullOrWhiteSpace(userId))
			{
				LastModificationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName))
			{
				LastModificationUserName = userName;
			}
		}

		protected ModificationAuditedEntity(TKey id) : base(id)
		{
		}
	}
}