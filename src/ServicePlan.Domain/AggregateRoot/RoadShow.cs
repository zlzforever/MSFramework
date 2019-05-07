using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class RoadShow : ValueObject
	{
		public Client Client { get; }

		public List<ClientUser> ClientUsers { get; }

		public Guid Owner { get; }

		public string Address { get; }

		public RoadShow(Client client, List<ClientUser> clientUsers, string address, Guid owner)
		{
			Client = client;
			ClientUsers = clientUsers;
			Owner = owner;
			Address = address;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Client;
			yield return ClientUsers;
			yield return Owner;
			yield return Address;
		}
	}
}