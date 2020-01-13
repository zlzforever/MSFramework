using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;

namespace MSFramework.Audit
{
	/// <summary>
	/// 实体属性审计信息
	/// </summary>
	public class AuditProperty : IEntity<Guid>
	{
		/// <summary>
		/// 获取或设置 所属审计实体编号
		/// </summary>
		[Required]
		public Guid AuditEntityId { get; set; }

		/// <summary>
		/// 获取或设置 所属审计实体
		/// </summary>
		public virtual AuditEntity AuditEntity { get; set; }

		/// <summary>
		/// 获取或设置 名称
		/// </summary>
		[StringLength(255)]
		public string DisplayName { get; set; }

		/// <summary>
		/// 获取或设置 字段
		/// </summary>
		[StringLength(255)]
		public string FieldName { get; set; }

		/// <summary>
		/// 获取或设置 旧值
		/// </summary>
		public string OriginalValue { get; set; }

		/// <summary>
		/// 获取或设置 新值
		/// </summary>
		public string NewValue { get; set; }

		/// <summary>
		/// 获取或设置 数据类型
		/// </summary>
		[StringLength(255)]
		public string DataType { get; set; }

		[StringLength(36)] public string ConcurrencyStamp { get; set; }

		public Guid Id { get; set; } = CombGuid.NewGuid();
	}
}