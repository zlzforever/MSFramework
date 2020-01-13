using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.Audit
{
	public class AuditEntity : IEntity<Guid>
	{
		/// <summary>¬
		/// 获取或设置 所属审计操作编号
		/// </summary>
		[Required]
		public Guid OperationId { get; set; }

		/// <summary>
		/// 获取或设置 所属审计操作
		/// </summary>
		public virtual AuditOperation Operation { get; set; }

		/// <summary>
		/// 获取或设置 实体名称
		/// </summary>
		[StringLength(255)]
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		public string DisplayName { get; set; }

		/// <summary>
		/// 获取或设置 类型名称
		/// </summary>
		[StringLength(255)]
		public string TypeName { get; set; }

		/// <summary>
		/// 获取或设置 数据编号
		/// </summary>
		[StringLength(64)]
		public string EntityKey { get; set; }

		/// <summary>
		/// 获取或设置 操作类型
		/// </summary>
		public OperateType OperateType { get; set; }

		/// <summary>
		/// 获取或设置 操作实体属性集合
		/// </summary>
		public virtual ICollection<AuditProperty> Properties { get; set; } = new List<AuditProperty>();

		public Guid Id { get; set; } = CombGuid.NewGuid();

		[StringLength(36)] public string ConcurrencyStamp { get; set; }
	}
}