using System;
using System.ComponentModel;

namespace MSFramework.Domain
{
	public interface IModificationAudited
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[DisplayName("修改者标识")]
		string LastModificationUserId { get; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[DisplayName("修改者名称")]
		string LastModificationUserName { get; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		[DisplayName("修改时间")]
		DateTimeOffset? LastModificationTime { get; }

		void SetModificationAudited(string userId, string userName, DateTimeOffset lastModificationTime = default);
	}
}