using EventBus;

namespace MSFramework.Audit
{
	public class AuditOperationEvent : Event
	{
		public AuditOperation AuditOperation { get; private set; }

		public AuditOperationEvent(AuditOperation auditOperation)
		{
			AuditOperation = auditOperation;
		}
	}
}