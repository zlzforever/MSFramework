using MSFramework.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MSFramework.Domain.Auditing
{
	public class CreationAuditedEntity<TKey> : EntityBase<TKey>, ICreationAudited
	{
		public virtual DateTime CreationTime { get; set; }

		public virtual string CreatorUserId { get; set; }
	}
}
