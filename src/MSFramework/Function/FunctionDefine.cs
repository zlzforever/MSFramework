using MSFramework.Common;
using MSFramework.Domain.AggregateRoot;

namespace MSFramework.Function
{
	public class FunctionDefine : ModificationAuditedAggregateRoot
	{
		public bool Enabled { get; private set; } = true;

		/// <summary>
		/// 功能名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 功能标识，必须是唯一的
		/// </summary>
		public string Code { get; private set; }

		/// <summary>
		/// 功能描述
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// 是否过期
		/// </summary>
		public bool Expired { get; private set; }

		public FunctionDefine(string name, string code, string description) : base(CombGuid.NewGuid())
		{
			Name = name;
			Code = code;
			Description = description;
		}

		public void Expire()
		{
			Expired = true;
		}

		public void Renewal()
		{
			Expired = false;
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'Name': {Name}, 'Code': {Code}, 'Enabled': {Enabled}, 'Expired': {Expired}, 'Description': {Description} }}";
		}
	}
}