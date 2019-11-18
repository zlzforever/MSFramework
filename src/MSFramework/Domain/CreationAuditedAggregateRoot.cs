using System;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public abstract class CreationAuditedAggregateRoot : AggregateRootBase, ICreationAudited
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		[Required]
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		[Required]
		[StringLength(255)]
		public string CreationUserId { get; private set; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
		[Required]
		[StringLength(255)]
		public string CreationUserName { get; private set; }

		public virtual void SetCreationAudited(string userId, string userName)
		{
			if (CreationTime == default)
			{
				CreationTime = DateTimeOffset.Now;
			}

			if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(CreationUserId))
			{
				CreationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(CreationUserName))
			{
				CreationUserName = userName;
			}
		}
	}
}