using System;

namespace MSFramework.Domain
{
	public interface IDeletionAudited
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		bool Deleted { get; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		string DeletionUserId { get; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		string DeletionUserName { get; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		DateTimeOffset? DeletionTime { get; set; }

		void Delete(string userId, string userName, DateTimeOffset deletionTime = default);
	}
}