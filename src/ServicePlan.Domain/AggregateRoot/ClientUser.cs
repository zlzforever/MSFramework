using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ClientUser : ValueObject
	{
		public Guid ClientId { get; }

		public string ClientName { get; }

		public string ClientShortName { get; }

		public Guid ClientUserId { get; }

		public string FirstName { get; }

		public string LastName { get; }

		public ClientUser(Guid clientUserId, string firstName, string lastName, Guid clientId, string clientName, string clientShortName)
		{
			ClientUserId = clientUserId;
			FirstName = firstName;
			LastName = lastName;
			ClientId = clientId;
			ClientName = clientName;
			ClientShortName = clientShortName;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return ClientUserId;
			yield return FirstName;
			yield return LastName;
			yield return ClientId;
			yield return ClientName;
			yield return ClientShortName;
		}
	}
}