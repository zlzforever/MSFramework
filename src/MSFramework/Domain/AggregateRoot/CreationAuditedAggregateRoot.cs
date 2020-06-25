using System;

namespace MSFramework.Domain.AggregateRoot
{
	public abstract class CreationAuditedAggregateRoot : CreationAuditedAggregateRoot<Guid>
	{
		protected CreationAuditedAggregateRoot(Guid id) : base(id)
		{
		}
	}

	public abstract class CreationAuditedAggregateRoot<TKey> : AggregateRootBase<TKey>, ICreationAudited
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		public string CreationUserId { get; private set; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
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

		protected CreationAuditedAggregateRoot(TKey id) : base(id)
		{
		}
	}
}