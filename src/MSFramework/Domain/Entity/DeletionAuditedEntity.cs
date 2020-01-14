using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain.Entity
{
	public abstract class DeletionAuditedEntity : DeletionAuditedEntity<Guid>
	{
	}

	public abstract class DeletionAuditedEntity<TKey> : EntityBase<TKey>, IDeletionAudited
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		[Required]
		[Description("是否已经删除")]
		public bool IsDeleted { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[StringLength(255)]
		[Description("删除者标识")]
		public string DeleterId { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[StringLength(255)]
		[Description("删除者名称")]
		public string DeleterName { get; private set; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		[Description("删除时间")]
		public DateTimeOffset? DeletionTime { get; set; }

		public void Delete(string userId, string userName, DateTimeOffset deletionTime = default)
		{
			// 删除只能一次操作，因此如果已经有值，不能再做设置
			if (!IsDeleted)
			{
				IsDeleted = true;

				if (DeletionTime == default)
				{
					DeletionTime = deletionTime == default ? DateTimeOffset.Now : deletionTime;
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