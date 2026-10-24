using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCore.CAP.Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MSFramework.Tests;

public class DaprConsumerClientTests
{
    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;

        public T Get(string name) => value;

        public IDisposable OnChange(Action<T, string> listener) => NullDisposable.Instance;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();

        public void Dispose()
        {
        }
    }

    private static StaticOptionsMonitor<DaprOptions> CreateOptions() =>
        new(new DaprOptions { Pubsub = "orders" });

    [Fact]
    public async Task SubscribeAsync_RegistersEveryTopicRoute()
    {
        var routes = new List<(string Path, string Pubsub, string Topic)>();
        var client = new DaprConsumerClient(
            "group-a",
            CreateOptions(),
            (path, pubsub, topic, _) => routes.Add((path, pubsub, topic)),
            NullLogger<DaprConsumerClient>.Instance);

        await client.SubscribeAsync(["order.created", "order.cancelled"]);

        Assert.Equal(2, routes.Count);
        Assert.Equal(("v1.0/publish/orders/order_created", "orders", "order.created"), routes[0]);
        Assert.Equal(("v1.0/publish/orders/order_cancelled", "orders", "order.cancelled"), routes[1]);
    }

    [Fact]
    public async Task RejectAsync_SetsBadRequestResponse()
    {
        var client = new DaprConsumerClient(
            "group-a",
            CreateOptions(),
            (_, _, _, _) => { },
            NullLogger<DaprConsumerClient>.Instance);
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };

        await client.RejectAsync(accessor);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task CreateAsyncAndLifecycleMethods_CompleteWithoutThrowing()
    {
        DaprConsumerClientFactory.MapEndpointRoute = (_, _, _, _) => { };

        try
        {
            var factory = new DaprConsumerClientFactory(CreateOptions(), NullLoggerFactory.Instance);
            var client = await factory.CreateAsync("group-a", 1);

            await client.ListeningAsync(TimeSpan.Zero, default);
            await client.CommitAsync(new object());
            await client.DisposeAsync();

            Assert.IsType<DaprConsumerClient>(client);
        }
        finally
        {
            DaprConsumerClientFactory.MapEndpointRoute = null;
        }
    }
}
