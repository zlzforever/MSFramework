using System.Threading.Tasks;
using EventBus;
using MSFramework.Audit;

namespace MSFramework.Function
{
	public class AuditOperationEventHandler : IEventHandler<AuditOperationEvent>
	{
		private readonly IAuditStore _auditStore;

		public AuditOperationEventHandler(IAuditStore auditStore)
		{
			_auditStore = auditStore;
		}

		public async Task HandleAsync(AuditOperationEvent @event)
		{
			await _auditStore.SaveAsync(@event.AuditOperation);
		}
	}
}