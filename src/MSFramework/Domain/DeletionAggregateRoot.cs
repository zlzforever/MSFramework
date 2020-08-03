using System;

namespace MSFramework.Domain
{
	public abstract class DeletionAggregateRoot : DeletionAggregateRoot<Guid>
	{
		protected DeletionAggregateRoot(Guid id) : base(id)
		{
		}
	}

	public abstract class DeletionAggregateRoot<TKey> : ModificationAggregateRoot<TKey>, IDeletion
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		public bool Deleted { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		public string DeletionUserId { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		public string DeletionUserName { get; private set; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		public DateTimeOffset? DeletionTime { get; set; }

		public void Delete(string userId, string userName, DateTimeOffset deletionTime = default)
		{
			// 删除只能一次操作，因此如果已经有值，不能再做设置
			if (!Deleted)
			{
				Deleted = true;

				if (DeletionTime == default)
				{
					DeletionTime = deletionTime == default ? DateTimeOffset.Now : deletionTime;
				}

				if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(DeletionUserId))
				{
					DeletionUserId = userId;
				}

				if (!string.IsNullOrWhiteSpace(userName) &&
				    string.IsNullOrWhiteSpace(DeletionUserName))
				{
					DeletionUserName = userName;
				}
			}
		}

		protected DeletionAggregateRoot(TKey id) : base(id)
		{
		}
	}
}