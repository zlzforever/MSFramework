using MSFramework.Domain;

namespace MSFramework.Function
{
	public class Function : ModificationAuditedAggregateRoot, IFunction
	{
		public bool Enabled { get; private set; }
		public string Name { get; private set; }
		public string Path { get; private set; }
		public bool AuditOperationEnabled { get; private set; }
		public bool AuditEntityEnabled { get; private set; }
	}
}