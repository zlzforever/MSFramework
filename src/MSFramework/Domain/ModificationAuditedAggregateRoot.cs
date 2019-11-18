using System;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public abstract class ModificationAuditedAggregateRoot : CreationAuditedAggregateRoot, IModificationAudited
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		public string LastModificationUserId { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		public string LastModificationUserName { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		public DateTimeOffset? LastModificationTime { get; private set; }

		public virtual void SetModificationAudited(string userId, string userName)
		{
			if (LastModificationTime == default)
			{
				LastModificationTime = DateTimeOffset.Now;
			}

			if (!string.IsNullOrWhiteSpace(userId) &&
			    string.IsNullOrWhiteSpace(LastModificationUserId))
			{
				LastModificationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName) &&
			    string.IsNullOrWhiteSpace(LastModificationUserName))
			{
				LastModificationUserName = userName;
			}
		}
	}
}