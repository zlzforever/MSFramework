using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public abstract class CreationAuditedAggregateRoot : AggregateRootBase, ICreationAudited
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		[Required]
		[Description("创建时间")]
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		[Required]
		[StringLength(255)]
		[Description("创建用户标识")]
		public string CreationUserId { get; private set; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
		[Required]
		[StringLength(255)]
		[Description("创建用户名称")]
		public string CreationUserName { get; private set; }

		public virtual void SetCreationAudited(string userId, string userName, DateTimeOffset creationTime = default)
		{
			if (CreationTime == default)
			{
				CreationTime = creationTime == default ? DateTimeOffset.Now : creationTime;
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