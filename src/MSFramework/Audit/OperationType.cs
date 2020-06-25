using MSFramework.Domain;

namespace MSFramework.Audit
{
	/// <summary>
	/// 表示实体审计操作类型
	/// </summary>
	public class OperationType : Enumeration
	{
		public static OperationType Query = new OperationType(1, nameof(Query));
		public static OperationType Add = new OperationType(1, nameof(Add));
		public static OperationType Modify = new OperationType(1, nameof(Modify));
		public static OperationType Delete = new OperationType(1, nameof(Delete));

		public OperationType(int id, string name) : base(id, name)
		{
		}
	}
}