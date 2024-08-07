using System.Net;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MSFramework.Tests;

public class HttpContextTests
{
    [Fact]
    public void GetRemoteIpAddressString_ReturnsForwardedForHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
        var result = context.GetRemoteIpAddressString();
        Assert.Equal("192.168.1.1", result);
    }

    [Fact]
    public void GetRemoteIpAddressString_ReturnsRemoteIpAddress_WhenForwardedForHeaderIsEmpty()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.2");
        var result = context.GetRemoteIpAddressString();
        Assert.Equal("192.168.1.2", result);
    }

    [Fact]
    public void GetRemoteIpAddressString_ReturnsNull_WhenNoIpAddressAvailable()
    {
        var context = new DefaultHttpContext();
        var result = context.GetRemoteIpAddressString();
        Assert.Null(result);
    }

    [Fact]
    public void GetRemoteIpAddress_ReturnsForwardedForHeaderAsIpAddress()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
        var result = context.GetRemoteIpAddress();
        Assert.Equal(IPAddress.Parse("192.168.1.1"), result);
    }

    [Fact]
    public void GetRemoteIpAddress_ReturnsRemoteIpAddress_WhenForwardedForHeaderIsEmpty()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.2");
        var result = context.GetRemoteIpAddress();
        Assert.Equal(IPAddress.Parse("192.168.1.2"), result);
    }

    [Fact]
    public void GetRemoteIpAddress_ReturnsNull_WhenNoIpAddressAvailable()
    {
        var context = new DefaultHttpContext();
        var result = context.GetRemoteIpAddress();
        Assert.Null(result);
    }
}
