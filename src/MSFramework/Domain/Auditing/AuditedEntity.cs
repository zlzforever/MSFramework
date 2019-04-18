using System;

namespace MSFramework.Domain.Auditing
{
	/// <summary>
	/// This class can be used to simplify implementing <see cref="IAuditedObject"/>.
	/// </summary>
	/// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
	[Serializable]
	public abstract class AuditedEntity<TKey> : CreationAuditedEntity<TKey>, IAudited where TKey : IEquatable<TKey>
	{
		public virtual DateTime? LastModificationTime { get; set; }

		public virtual string LastModifierUserId { get; set; }
	}
}