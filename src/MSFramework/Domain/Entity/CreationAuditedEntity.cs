using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain.Entity
{
	public abstract class CreationAuditedEntity : CreationAuditedEntity<Guid>
	{
	}

	public abstract class CreationAuditedEntity<TKey> :
		EntityBase<TKey>, ICreationAudited
		where TKey : IEquatable<TKey>
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
			// 创建只能一次操作，因此如果已经有值，不能再做设置
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