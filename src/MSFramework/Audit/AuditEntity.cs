using System.Collections.Generic;

namespace MSFramework.Audit
{
	public class AuditEntity : AuditBase
	{
		/// <summary>
		/// 获取或设置 实体名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 获取或设置 类型名称
		/// </summary>
		public string TypeName { get; private set; }

		/// <summary>
		/// 获取或设置 数据编号
		/// </summary>
		public string EntityKey { get; private set; }

		/// <summary>
		/// 获取或设置 操作类型
		/// </summary>
		public OperateType OperateType { get; private set; }

		/// <summary>
		/// 获取或设置 操作实体属性集合
		/// </summary>
		public ICollection<AuditProperty> PropertyEntries { get; set; }
	}
}