using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class AuditHistory : ValueObject
	{
		private User _user;

		private string _operation;

		private string _result;

		public AuditHistory(User user, string operation, string result)
		{
			_user = user;
			_operation = operation;
			_result = result;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return _user;
			yield return _operation;
			yield return _result;
		}
	}
}