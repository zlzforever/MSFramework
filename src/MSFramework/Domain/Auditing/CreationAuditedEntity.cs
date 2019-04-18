using MSFramework.Domain.Entity;
using System;

namespace MSFramework.Domain.Auditing
{
	public class CreationAuditedEntity<TKey> : EntityBase<TKey>, ICreationAudited where TKey : IEquatable<TKey>
	{
		public virtual DateTime CreationTime { get; set; }

		public virtual string CreatorUserId { get; set; }
	}
}