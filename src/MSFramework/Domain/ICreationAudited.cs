using System;
using System.ComponentModel;

namespace MSFramework.Domain
{
	public interface ICreationAudited
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		[DisplayName("创建时间")]
		DateTimeOffset CreationTime { get; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		[DisplayName("创建者标识")]
		string CreationUserId { get; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
		[DisplayName("创建者名称")]
		string CreationUserName { get; }

		void SetCreationAudited(string userId, string userName, DateTimeOffset creationTime = default);
	}
}