using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain
{
	public abstract class CreationAggregateRoot : CreationAggregateRoot<ObjectId>
	{
		protected CreationAggregateRoot(ObjectId id) : base(id)
		{
		}
	}

	public abstract class CreationAggregateRoot<TKey> : AggregateRoot<TKey>, ICreation where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTimeOffset? CreationTime { get; private set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		public string CreatorId { get; private set; }

		public virtual void SetCreation(string userId, DateTimeOffset creationTime = default)
		{
			// 创建只能一次操作，因此如果已经有值，不能再做设置
			if (CreationTime == default)
			{
				CreationTime = creationTime == default ? DateTimeOffset.Now : creationTime;
			}

			if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(CreatorId))
			{
				CreatorId = userId;
			}
		}

		protected CreationAggregateRoot(TKey id) : base(id)
		{
		}
	}
}