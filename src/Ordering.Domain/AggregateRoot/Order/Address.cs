using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot.Order
{
	public class Address : ValueObject
	{
		public string Street { get; }

		public string City { get; }

		public string State { get; }

		public string Country { get; }

		public string ZipCode { get; }

		private Address()
		{
		}

		public Address(string street, string city, string state, string country, string zipcode)
		{
			Street = street;
			City = city;
			State = state;
			Country = country;
			ZipCode = zipcode;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			// Using a yield return statement to return each element one at a time
			yield return Street;
			yield return City;
			yield return State;
			yield return Country;
			yield return ZipCode;
		}
	}
}