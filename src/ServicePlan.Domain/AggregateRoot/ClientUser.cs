using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ClientUser : ValueObject
	{
		public Guid Id { get; }

		public string FirstName { get; }

		public string LastName { get; }

		public ClientUser(Guid id, string firstName, string lastName)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return FirstName;
			yield return LastName;
		}
	}
}