using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class AuditState : Enumeration
	{
		public static AuditState AwaitingValidation = new AuditState(1, nameof(AwaitingValidation).ToLowerInvariant());
		public static AuditState Confirmed = new AuditState(2, nameof(Confirmed).ToLowerInvariant());
		public static AuditState Dismissed = new AuditState(3, nameof(Dismissed).ToLowerInvariant());
		
		public AuditState(int id, string name) : base(id, name)
		{
		}
	}
}