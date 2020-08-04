using MSFramework.Common;
using MSFramework.Domain;

namespace MSFramework.Audits
{
	/// <summary>
	/// 实体属性审计信息
	/// </summary>
	public class AuditProperty : EntityBase<ObjectId>
	{
		private AuditProperty() : base(ObjectId.NewId())
		{
		}

		public AuditProperty(string propertyName, string propertyType, string originalValue, string newValue)
			: this()
		{
			Name = propertyName;
			Type = propertyType;
			OriginalValue = originalValue;
			NewValue = newValue;
		}

		/// <summary>
		/// 所属实体
		/// </summary>
		public virtual AuditEntity Entity { get; internal set; }

		/// <summary>
		/// 获取或设置 字段
		/// </summary>
		public virtual string Name { get; private set; }

		/// <summary>
		/// 获取或设置 数据类型
		/// </summary>
		public virtual string Type { get; private set; }

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
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'PropertyName': {Name}, 'PropertyType': {Type}, 'OriginalValue': {OriginalValue}, 'NewValue': {NewValue} }}";
		}
	}
}