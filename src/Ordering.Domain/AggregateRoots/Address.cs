using System.Collections.Generic;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public record Address : ValueObject
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string ZipCode { get; set; }

    public override string ToString()
    {
        return $"{Country} {State} {City} {Street} {ZipCode}";
    }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    // protected override IEnumerable<object> GetEqualityComponents()
    // {
    //     // Using a yield return statement to return each element one at a time
    //     yield return Street;
    //     yield return City;
    //     yield return State;
    //     yield return Country;
    //     yield return ZipCode;
    // }
}
