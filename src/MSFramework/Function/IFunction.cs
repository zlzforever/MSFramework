using System;
using MSFramework.Domain;

namespace MSFramework.Function
{
	public interface IFunction : IEntity<Guid>, IModificationAudited
	{
		bool Enabled { get; }

		/// <summary>
		/// 功能名称，唯一
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 功能路径，唯一
		/// </summary>
		string Path { get; }

		/// <summary>
		/// 获取或设置 是否启用操作审计
		/// </summary>
		bool AuditOperationEnabled { get; }

		/// <summary>
		/// 获取或设置 是否启用数据审计
		/// </summary>
		bool AuditEntityEnabled { get; }
	}
}