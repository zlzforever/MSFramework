using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using Xunit;

namespace MSFramework.Tests
{
	public class ValueObjectTests
	{
		public class Address : ValueObject
		{
			public String Street { get; private set; }
			public String City { get; private set; }
			public String State { get; private set; }
			public String Country { get; private set; }
			public String ZipCode { get; private set; }

			public Address()
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

		[Fact]
		public void ValueObjectEqual()
		{
			var address1 = new Address("a", "b", "c", "d", "e");
			var address2 = new Address("a", "b", "c", "d", "e");
			Assert.True(address1 == address2);
			Assert.True(address1.Equals(address2));
		}

		[Fact]
		public void ValueObjectNotEqual()
		{
			var address1 = new Address("a", "b", "c", "d", "e");
			var address3 = new Address("a", "b", "c", "d", "f");
			Assert.True(address1 != address3);
		}

		[Fact]
		public void ValueObjectCopy()
		{
			var address1 = new Address("a", "b", "c", "d", "e");
			var address2 = address1.Clone();
			Assert.True(address1 == address2);
		}
	}
}