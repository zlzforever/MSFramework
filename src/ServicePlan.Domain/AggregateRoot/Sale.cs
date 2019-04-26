using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class Sale : ValueObject
	{
		public string Id { get; private set; }

		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return FirstName;
			yield return LastName;
		}
	}
}