using System;

namespace MicroserviceFramework.Domain
{
	public interface IDeletion : ISoftDelete
	{
		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		string DeleterId { get; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		DateTimeOffset? DeletionTime { get; set; }

		void Delete(string userId, DateTimeOffset deletionTime = default);
	}
}