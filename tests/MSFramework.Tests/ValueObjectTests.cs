using MicroserviceFramework.Domain;
using Xunit;

namespace MSFramework.Tests;

public class ValueObjectTests
{
    public record V1(string V) : ValueObject
    {
        public string V { get; private set; } = V;

        // protected override IEnumerable<object> GetEqualityComponents()
        // {
        //     yield return V;
        // }
    }

    public record Address(string Street, string City, string State, string Country, string ZipCode)
        : ValueObject;


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
        var address2 = (Address)address1.Copy();

        Assert.True(Equals(address1, address2));
    }

    [Fact]
    public void ValueObjectToString()
    {
        var v1 = new V1("a").ToString();
        var v2 = new Address("a", "b", "c", "d", "f").ToString();

        Assert.Equal("V1 { V = a }", v1);
        Assert.Equal("Address { Street = a, City = b, State = c, Country = d, ZipCode = f }", v2);
    }
}
