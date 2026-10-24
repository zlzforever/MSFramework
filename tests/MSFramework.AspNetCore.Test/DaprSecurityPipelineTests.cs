#nullable enable

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.AspNetCore.Test;

public class DaprSecurityPipelineTests
{
    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("wrong-token", HttpStatusCode.Unauthorized)]
    [InlineData("expected-token", HttpStatusCode.OK)]
    public async Task TopicEndpointValidatesTokenThroughTestServer(
        string? requestToken,
        HttpStatusCode expectedStatusCode)
    {
        using var server = CreateServer(IPAddress.Loopback);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/topic");
        if (requestToken != null)
        {
            request.Headers.Add("dapr-app-token", requestToken);
        }

        using var response = await server.CreateClient().SendAsync(request);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task TopicEndpointRejectsSpoofedForwardedHeaderThroughTestServer()
    {
        using var server = CreateServer(IPAddress.Parse("203.0.113.10"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "/topic");
        request.Headers.Add("dapr-app-token", "expected-token");
        request.Headers.Add("X-Forwarded-For", "127.0.0.1");

        using var response = await server.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithoutEndpointIsRejectedThroughTestServer()
    {
        using var server = CreateServer(IPAddress.Loopback);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/missing");
        request.Headers.Add("dapr-app-token", "expected-token");

        using var response = await server.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static TestServer CreateServer(IPAddress remoteIpAddress)
    {
        var builder = new WebHostBuilder()
            .ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["DAPR_APP_TOKEN"] = "expected-token"
                }))
            .ConfigureServices(services => services.AddRouting())
            .Configure(app =>
            {
                app.UseRouting();
                app.Use((context, next) =>
                {
                    context.Connection.RemoteIpAddress = remoteIpAddress;
                    return next(context);
                });
                app.UseMiddleware<DaprSecurityMiddleware>();
                app.UseEndpoints(endpoints => endpoints
                    .MapPost("/topic", context => context.Response.WriteAsync("ok"))
                    .WithMetadata(new TopicAttribute("pubsub", "topic")));
            });

        return new TestServer(builder);
    }
}
