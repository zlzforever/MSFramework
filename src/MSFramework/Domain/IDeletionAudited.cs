using System;
using System.ComponentModel;

namespace MSFramework.Domain
{
	public interface IDeletionAudited
	{
		/// <summary>
		/// 是否已经删除
		/// </summary>
		[DisplayName("是否已经删除")]
		bool IsDeleted { get; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[DisplayName("删除者标识")]
		string DeleterId { get; }

		/// <summary>
		/// Which user deleted this entity?
		/// </summary>
		[DisplayName("删除者名称")]
		string DeleterName { get; }

		/// <summary>
		/// Deletion time of this entity.
		/// </summary>
		[DisplayName("删除时间")]
		DateTimeOffset? DeletionTime { get; set; }

		void Delete(string userId, string userName, DateTimeOffset deletionTime = default);
	}
}