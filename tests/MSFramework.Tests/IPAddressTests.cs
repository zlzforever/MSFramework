using System.Net;
using MicroserviceFramework.Extensions;
using Xunit;

namespace MSFramework.Tests;

public class IPAddressTests
{
    [Fact]
    void IsPrivate_ReturnsTrue_ForLoopbackAddress()
    {
        var address = IPAddress.Loopback;
        var result = address.IsPrivate();
        Assert.True(result);
    }

    [Fact]
    void IsPrivate_ReturnsTrue_For10DotAddress()
    {
        var address = IPAddress.Parse("10.0.0.1");
        var result = address.IsPrivate();
        Assert.True(result);
    }

    [Fact]
    void IsPrivate_ReturnsTrue_For172Dot16To31Address()
    {
        var address = IPAddress.Parse("172.16.0.1");
        var result = address.IsPrivate();
        Assert.True(result);

        address = IPAddress.Parse("172.31.255.255");
        result = address.IsPrivate();
        Assert.True(result);
    }

    [Fact]
    void IsPrivate_ReturnsTrue_For192Dot168Address()
    {
        var address = IPAddress.Parse("192.168.0.1");
        var result = address.IsPrivate();
        Assert.True(result);
    }

    [Fact]
    void IsPrivate_ReturnsFalse_ForPublicAddress()
    {
        var address = IPAddress.Parse("8.8.8.8");
        var result = address.IsPrivate();
        Assert.False(result);
    }

    [Fact]
    void IsPrivate_ReturnsFalse_ForIPv6Address()
    {
        var address = IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        var result = address.IsPrivate();
        Assert.False(result);
    }
}
