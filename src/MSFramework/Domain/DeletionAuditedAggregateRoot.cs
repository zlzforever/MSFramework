using System;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public abstract class DeletionAuditedAggregateRoot : ModificationAuditedAggregateRoot, IDeletionAudited
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		[Required]
		public bool IsDeleted { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[StringLength(255)]
		public string DeleterId { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[StringLength(255)]
		public string DeleterName { get; private set; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		public DateTimeOffset? DeletionTime { get; set; }

		public void Delete(string userId, string userName)
		{
			if (!IsDeleted)
			{
				IsDeleted = true;

				if (DeletionTime == default)
				{
					DeletionTime = DateTimeOffset.Now;
				}

				if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(DeleterId))
				{
					DeleterId = userId;
				}

				if (!string.IsNullOrWhiteSpace(userName) &&
				    string.IsNullOrWhiteSpace(DeleterName))
				{
					DeleterName = userName;
				}
			}
		}
	}
}