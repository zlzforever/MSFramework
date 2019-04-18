using System;
using System.Collections.Generic;
using System.Text;

namespace MSFramework.Domain.Auditing
{
	/// <summary>
	/// Implements <see cref="IFullAuditedObject"/> to be a base class for full-audited entities.
	/// </summary>
	/// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAudited
	{
		public virtual bool IsDeleted { get; set; }
		public virtual DateTime? DeletionTime { get; set; }
		public string DeleterUserId { get; set; }
	}
}
