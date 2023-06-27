using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class Address : ValueObject<Address>
{
    public string Street { get; private set; }

    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    private Address()
    {
    }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }
}
