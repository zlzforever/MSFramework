using System;

namespace MSFramework.Domain
{
	public interface IModificationAudited
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string LastModificationUserId { get; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string LastModificationUserName { get; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		DateTimeOffset? LastModificationTime { get; }

		void SetModificationAudited(string userId, string userName);
	}
}