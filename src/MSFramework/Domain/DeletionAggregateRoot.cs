using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain
{
	public abstract class DeletionAggregateRoot : DeletionAggregateRoot<ObjectId>
	{
		protected DeletionAggregateRoot(ObjectId id) : base(id)
		{
		}
	}

	public abstract class DeletionAggregateRoot<TKey> : ModificationAggregateRoot<TKey>, IDeletion
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		public bool IsDeleted { get; private set; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		public string DeleterId { get; private set; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		public DateTimeOffset? DeletionTime { get; set; }

		public virtual void Delete(string userId, DateTimeOffset deletionTime = default)
		{
			// 删除只能一次操作，因此如果已经有值，不能再做设置
			if (!IsDeleted)
			{
				IsDeleted = true;

				DeletionTime ??= deletionTime == default ? DateTimeOffset.Now : deletionTime;

				if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(DeleterId))
				{
					DeleterId = userId;
				}
			}
		}

		protected DeletionAggregateRoot(TKey id) : base(id)
		{
		}
	}
}