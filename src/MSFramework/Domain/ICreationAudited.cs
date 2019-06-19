using System;

namespace MSFramework.Domain
{
	public interface ICreationAudited
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		DateTimeOffset CreationTime { get; set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		string CreationUserId { get; set; }
	}
}