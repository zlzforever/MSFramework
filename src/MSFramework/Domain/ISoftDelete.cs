using System;

namespace MSFramework.Domain
{
	public interface ISoftDelete
	{
		bool IsDeleted { get; set; }
		
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string DeleteUserId { get; set; }
		
		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		DateTimeOffset? DeletionTime { get; set; }
	}
}