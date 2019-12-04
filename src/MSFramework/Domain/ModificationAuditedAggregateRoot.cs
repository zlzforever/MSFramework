using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public abstract class ModificationAuditedAggregateRoot : CreationAuditedAggregateRoot, IModificationAudited
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		[Description("最后修改者标识")]
		public string LastModificationUserId { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		[Description("最后修改者名称")]
		public string LastModificationUserName { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		[Description("最后修改时间")]
		public DateTimeOffset? LastModificationTime { get; private set; }

		public virtual void SetModificationAudited(string userId, string userName,
			DateTimeOffset lastModificationTime = default)
		{
			if (LastModificationTime == default)
			{
				LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;
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