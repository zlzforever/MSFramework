using System;

namespace MSFramework.Domain
{
	public interface IDeletionAudited
	{
		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		string DeleterUserId { get; set; }
		
		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		DateTime? DeletionTime { get; set; }
	}
}