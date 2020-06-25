using System;
using MSFramework.Common;
using MSFramework.Domain.Entity;

namespace MSFramework.Audit
{
	/// <summary>
	/// 实体属性审计信息
	/// </summary>
	public class AuditedProperty : EntityBase<Guid>
	{
		private AuditedProperty() : base(CombGuid.NewGuid())
		{
		}

		public AuditedProperty(string propertyName, string propertyType, string originalValue, string newValue)
			: this()
		{
			PropertyName = propertyName;
			PropertyType = propertyType;
			OriginalValue = originalValue;
			NewValue = newValue;
		}

		/// <summary>
		/// 获取或设置 字段
		/// </summary>
		public virtual string PropertyName { get; private set; }

		/// <summary>
		/// 获取或设置 数据类型
		/// </summary>
		public virtual string PropertyType { get; private set; }

		/// <summary>
		/// 获取或设置 旧值
		/// </summary>
		public virtual string OriginalValue { get; private set; }

		/// <summary>
		/// 获取或设置 新值
		/// </summary>
		public virtual string NewValue { get; private set; }
		
		public override string ToString()
		{
			return $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'PropertyName': {PropertyName}, 'PropertyType': {PropertyType}, 'OriginalValue': {OriginalValue}, 'NewValue': {NewValue} }}";
		}
	}
}