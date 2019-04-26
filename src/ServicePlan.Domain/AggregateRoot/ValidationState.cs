using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ValidationState : Enumeration
	{
		public static ValidationState AwaitingValidation = new ValidationState(1, nameof(AwaitingValidation).ToLowerInvariant());
		public static ValidationState Confirmed = new ValidationState(2, nameof(Confirmed).ToLowerInvariant());
		public static ValidationState Dismissed = new ValidationState(3, nameof(Dismissed).ToLowerInvariant());
		
		public ValidationState(int id, string name) : base(id, name)
		{
		}
	}
}