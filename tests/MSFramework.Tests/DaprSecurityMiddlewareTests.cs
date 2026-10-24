using System.Net;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.Tests;

public class DaprSecurityMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_TopicFromRemoteClientWithSpoofedLoopbackHeader_ReturnsForbidden()
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance);
        var context = CreateTopicContext(IPAddress.Parse("203.0.113.10"));
        context.Request.Headers["X-Forwarded-For"] = "127.0.0.1";

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_TopicFromLoopbackConnection_InvokesNext()
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance);
        var context = CreateTopicContext(IPAddress.Loopback);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    private static DefaultHttpContext CreateTopicContext(IPAddress remoteIpAddress)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = remoteIpAddress;
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new TopicAttribute("pubsub", "topic")),
            "topic"));
        return context;
    }
}
