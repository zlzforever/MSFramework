using System;

namespace MSFramework.Domain
{
	public interface IModification
	{
		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string ModificationUserId { get; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		string ModificationUserName { get; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		DateTimeOffset? ModificationTime { get; }

		void SetModification(string userId, string userName, DateTimeOffset modificationTime = default);
	}
}