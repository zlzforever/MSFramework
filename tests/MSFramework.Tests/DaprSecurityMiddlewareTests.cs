using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.Tests;

public class DaprSecurityMiddlewareTests
{
    [Fact]
    public async Task InvokeAsyncTopicFromRemoteClientWithSpoofedLoopbackHeaderReturnsForbidden()
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance,
            CreateConfiguration("expected-token"));
        var context = CreateTopicContext(IPAddress.Parse("203.0.113.10"));
        context.Request.Headers["X-Forwarded-For"] = "127.0.0.1";
        context.Request.Headers["dapr-app-token"] = "expected-token";

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsyncTopicFromLoopbackConnectionWithCorrectTokenInvokesNext()
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance,
            CreateConfiguration("expected-token"));
        var context = CreateTopicContext(IPAddress.Loopback);
        context.Request.Headers["dapr-app-token"] = "expected-token";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-token")]
    public async Task InvokeAsyncTopicWithoutCorrectTokenReturnsUnauthorized(string token)
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance,
            CreateConfiguration("expected-token"));
        var context = CreateTopicContext(IPAddress.Loopback);
        if (token != null)
        {
            context.Request.Headers["dapr-app-token"] = token;
        }

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsyncWithoutEndpointFailsClosed()
    {
        var nextCalled = false;
        var middleware = new DaprSecurityMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<DaprSecurityMiddleware>.Instance,
            CreateConfiguration("expected-token"));
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Request.Headers["dapr-app-token"] = "expected-token";

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
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

    private static IConfiguration CreateConfiguration(string token)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["DAPR_APP_TOKEN"] = token
            })
            .Build();
    }
}
