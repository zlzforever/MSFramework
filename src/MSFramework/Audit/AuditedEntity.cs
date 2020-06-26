using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Entity;

namespace MSFramework.Audit
{
	public class AuditedEntity : EntityBase<Guid>
	{
		public AuditedEntity(string typeName, string entityId, OperationType operationType) : this()
		{
			TypeName = typeName;
			EntityId = entityId;
			OperationType = operationType;
		}

		private AuditedEntity() : base(CombGuid.NewGuid())
		{
			Properties = new List<AuditedProperty>();
		}

		/// <summary>
		/// 获取或设置 类型名称
		/// </summary>
		public string TypeName { get; private set; }

		/// <summary>
		/// 获取或设置 数据编号
		/// </summary>
		public string EntityId { get; private set; }

		/// <summary>
		/// 获取或设置 操作类型
		/// </summary>
		public OperationType OperationType { get; private set; }

		/// <summary>
		/// 获取或设置 操作实体属性集合
		/// </summary>
		public virtual ICollection<AuditedProperty> Properties { get; private set; }

		public void AddProperties(IEnumerable<AuditedProperty> properties)
		{
			foreach (var entity in properties)
			{
				Properties.Add(entity);
			}
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'TypeName': {TypeName}, 'EntityId': {EntityId}, 'OperateType': {OperationType} }}";
		}
	}
}