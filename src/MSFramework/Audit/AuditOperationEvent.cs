namespace MSFramework.Audit
{
	public class AuditOperationEvent : MSFramework.EventBus.Event
	{
		public AuditOperation AuditOperation { get; }

		public AuditOperationEvent(AuditOperation auditOperation)
		{
			AuditOperation = auditOperation;
		}
	}
}