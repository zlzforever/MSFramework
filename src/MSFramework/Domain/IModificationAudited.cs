using System;

namespace MSFramework.Domain
{
	public interface IModificationAudited
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string LastModifierUserId { get; set; }
		
		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		DateTime? LastModificationTime { get; set; }
	}
}