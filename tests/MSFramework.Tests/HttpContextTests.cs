using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

    [Fact]
    public async Task GetBodyTextAsyncRejectsOversizedBodyAndRestoresPosition()
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("0123456789"));
        context.Request.Body.Position = 3;
        context.Request.ContentLength = null;
        var accessor = new HttpContextAccessor { HttpContext = context };

        await Assert.ThrowsAsync<InvalidDataException>(() => accessor.GetBodyTextAsync(5));

        Assert.Equal(3, context.Request.Body.Position);
    }

    [Fact]
    public async Task GetBodyTextAsyncRejectsKnownOversizedBodyBeforeReading()
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("0123456789"));
        context.Request.Body.Position = 4;
        context.Request.ContentLength = 10;
        var accessor = new HttpContextAccessor { HttpContext = context };

        await Assert.ThrowsAsync<InvalidDataException>(() => accessor.GetBodyTextAsync(5));

        Assert.Equal(4, context.Request.Body.Position);
    }
}
