using System;

namespace MSFramework.Audit
{
	public abstract class AuditBase
	{
		public Guid Id { get; set; }

		/// <summary>
		/// 获取或设置 当前用户标识
		/// </summary>
		public string UserId { get; set; }

		/// <summary>
		/// 获取或设置 当前用户名
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 获取或设置 当前用户昵称
		/// </summary>
		public string NickName { get; set; }

		/// <summary>
		/// 获取或设置 信息添加时间
		/// </summary>
		public DateTimeOffset CreatedTime { get; set; }
	}
}