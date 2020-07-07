using MSFramework.Domain;

namespace MSFramework.Audit
{
	/// <summary>
	/// 表示实体审计操作类型
	/// </summary>
	public class OperationType : Enumeration
	{
		public static OperationType Query = new OperationType(nameof(Query), nameof(Query));
		public static OperationType Add = new OperationType(nameof(Add), nameof(Add));
		public static OperationType Modify = new OperationType(nameof(Modify), nameof(Modify));
		public static OperationType Delete = new OperationType(nameof(Delete), nameof(Delete));

		public OperationType(string id, string name) : base(id, name)
		{
		}
	}
}