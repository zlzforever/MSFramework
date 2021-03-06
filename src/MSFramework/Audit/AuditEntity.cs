using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Audit
{
	public class AuditEntity : EntityBase<ObjectId>
	{
		public AuditEntity(string typeName, string entityId, OperationType operationType) : this()
		{
			Type = typeName;
			EntityId = entityId;
			OperationType = operationType;
		}

		private AuditEntity() : base(ObjectId.NewId())
		{
			Properties = new List<AuditProperty>();
		}

		/// <summary>
		/// 所属的操作
		/// </summary>
		public AuditOperation Operation { get; internal set; }

		/// <summary>
		/// 获取或设置 类型名称
		/// </summary>
		public string Type { get; private set; }

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
		public virtual ICollection<AuditProperty> Properties { get; private set; }

		public void AddProperties(IEnumerable<AuditProperty> properties)
		{
			foreach (var property in properties)
			{
				property.Entity = this;
				Properties.Add(property);
			}
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'TypeName': {Type}, 'EntityId': {EntityId}, 'OperateType': {OperationType} }}";
		}
	}
}