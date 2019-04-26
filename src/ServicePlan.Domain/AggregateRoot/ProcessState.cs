using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ProcessState : Enumeration
	{
		public static ValidationState AwaitingSubmit = new ValidationState(1, nameof(AwaitingSubmit).ToLowerInvariant());
		public static ValidationState Confirmed = new ValidationState(2, nameof(Confirmed).ToLowerInvariant());
		public static ValidationState Dismissed = new ValidationState(3, nameof(Dismissed).ToLowerInvariant());
		
		public ProcessState(int id, string name) : base(id, name)
		{
		}
	}
}