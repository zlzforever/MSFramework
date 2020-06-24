using System.ComponentModel.DataAnnotations;
using MSFramework.Domain.AggregateRoot;

namespace MSFramework.Function
{
	public class FunctionDefine : ModificationAuditedAggregateRoot, IFunction
	{
		public bool Enabled { get; private set; } = true;

		/// <summary>
		/// 功能名称，不唯一
		/// </summary>
		[StringLength(255)]
		public string Name { get; private set; }

		/// <summary>
		/// 功能路径，唯一
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Path { get; private set; }

		/// <summary>
		/// 功能描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; private set; }

		/// <summary>
		/// 是否过期
		/// </summary>
		public bool Expired { get; private set; }

		/// <summary>
		/// 获取或设置 是否启用操作审计
		/// </summary>
		public bool AuditOperationEnabled { get; private set; }

		/// <summary>
		/// 获取或设置 是否启用数据审计
		/// </summary>
		public bool AuditEntityEnabled { get; private set; }

		public FunctionDefine(string name, string path, string description, bool auditOperationEnabled = true,
			bool auditEntityEnabled = true)
		{
			Name = name;
			Path = path;
			Description = description;
			AuditOperationEnabled = auditOperationEnabled;
			AuditEntityEnabled = auditEntityEnabled;
		}

		public void Expire()
		{
			Expired = true;
		}

		public void Renewal()
		{
			Expired = false;
		}
	}
}