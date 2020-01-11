namespace MSFramework.Audit
{
	/// <summary>
	/// 实体属性审计信息
	/// </summary>
	public class AuditProperty
	{
		/// <summary>
		/// 获取或设置 名称
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// 获取或设置 字段
		/// </summary>
		public string FieldName { get; private set; }

		/// <summary>
		/// 获取或设置 旧值
		/// </summary>
		public string OriginalValue { get; private set; }

		/// <summary>
		/// 获取或设置 新值
		/// </summary>
		public string NewValue { get; private set; }

		/// <summary>
		/// 获取或设置 数据类型
		/// </summary>
		public string DataType { get; private set; }
	}
}