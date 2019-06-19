using System;
using System.ComponentModel.DataAnnotations;

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
		DateTimeOffset? DeletionTime { get; set; }
	}
}