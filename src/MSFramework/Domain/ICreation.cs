using System;

namespace MSFramework.Domain
{
	public interface ICreation
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		DateTimeOffset? CreationTime { get; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		string CreationUserId { get; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
		string CreationUserName { get; }

		void SetCreation(string userId, string userName, DateTimeOffset creationTime = default);
	}
}