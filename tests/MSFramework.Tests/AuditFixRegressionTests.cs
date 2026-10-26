using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.LocalEvent;
using MicroserviceFramework.Text.Json.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ordering.Domain;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Order;
using Xunit;

namespace MSFramework.Tests;

/// <summary>审查修复回归：主键比较、JSON 枚举、发布背压及订单项金额边界；不依赖外部服务。</summary>
public class AuditFixRegressionTests
{
    /// <summary>空主键与有效主键的比较应对称返回 false，同一瞬态实例保持自反。</summary>
    [Fact]
    public void NullKeyComparisonIsSymmetric()
    {
        var transient = new UserInfo(null);
        var persisted = new UserInfo("user-1");
        Assert.False(transient.Equals((object)persisted));
        Assert.False(persisted.Equals((object)transient));
        Assert.True(transient.Equals((object)transient));
    }

    /// <summary>非法枚举字符串或非字符串 JSON 必须产生输入格式异常。</summary>
    /// <param name="json">待校验的非法 JSON 值。</param>
    [Theory]
    [InlineData("\"not-a-status\"")]
    [InlineData("123")]
    [InlineData("{}")]
    public void InvalidEnumerationThrowsJsonException(string json)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumerationJsonConverter<OrderStatus>());
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderStatus>(json, options));
    }

    /// <summary>合法枚举应继续解析为共享静态实例。</summary>
    [Fact]
    public void ValidEnumerationKeepsSharedInstance()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumerationJsonConverter<OrderStatus>());
        Assert.Same(OrderStatus.Paid, JsonSerializer.Deserialize<OrderStatus>("\"Paid\"", options));
    }

    /// <summary>满队列的发布超时，不改变调用方取消的异常语义。</summary>
    [Fact]
    public async Task FullQueueHasBoundedPublishWaitAndPreservesCancellation()
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher(options =>
            options.PublishTimeout = TimeSpan.FromMilliseconds(30)));
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        // 不启动后台消费者，让容量确定性耗尽，不通过任意 Sleep 制造竞态。
        for (var index = 0; index < 2000; index++)
        {
            await publisher.PublishAsync(new ProbeEvent());
        }

        await Assert.ThrowsAsync<TimeoutException>(() => publisher.PublishAsync(new ProbeEvent()));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            publisher.PublishAsync(new ProbeEvent(), cancelled.Token));
        Assert.Equal(cancelled.Token, exception.CancellationToken);
    }

    /// <summary>发布超时不允许零、负数或超出定时器范围。</summary>
    /// <param name="milliseconds">非法超时时长，单位毫秒。</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4294967295d)]
    public void InvalidPublishTimeoutIsRejected(double milliseconds)
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher(options =>
            options.PublishTimeout = TimeSpan.FromMilliseconds(milliseconds)));
        using var provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<LocalEventOptions>>().Value);
    }

    /// <summary>接口级超时覆盖全局等待上限且不修改配置，调用中取消仍抛出取消异常。</summary>
    [Fact]
    public async Task PerPublishTimeoutOverridesDefaultWithoutChangingOptions()
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher(options =>
            options.PublishTimeout = TimeSpan.FromDays(1)));
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        // 不启动消费者，先验证显式超时的成功路径，再确定性填满通道。
        for (var index = 0; index < 2000; index++)
        {
            await publisher.PublishAsync(new ProbeEvent(), TimeSpan.FromSeconds(5));
        }

        var timedPublish = publisher.PublishAsync(new ProbeEvent(), TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<TimeoutException>(() => timedPublish.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(timedPublish.IsFaulted);
        Assert.Equal(TimeSpan.FromDays(1), provider.GetRequiredService<IOptions<LocalEventOptions>>().Value.PublishTimeout);

        using var cancellation = new CancellationTokenSource();
        var pending = publisher.PublishAsync(new ProbeEvent(), TimeSpan.FromDays(1), cancellation.Token);
        Assert.False(pending.IsCompleted);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    /// <summary>未配置时等待上限为 30 秒，接口允许显式传入空超时并正常入队。</summary>
    [Fact]
    public async Task NullPublishTimeoutUsesThirtySecondDefault()
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher());
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        Assert.Equal(TimeSpan.FromSeconds(30), provider.GetRequiredService<IOptions<LocalEventOptions>>().Value.PublishTimeout);
        await publisher.PublishAsync(new ProbeEvent(), timeout: null);
    }

    /// <summary>空超时应采用用户配置的等待上限，而不是另行硬编码 30 秒。</summary>
    [Fact]
    public async Task NullPublishTimeoutUsesConfiguredTimeout()
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher(options =>
            options.PublishTimeout = TimeSpan.FromMilliseconds(30)));
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        // 不启动消费者，使空超时的回退行为可通过满通道确定性验证。
        for (var index = 0; index < 2000; index++)
        {
            await publisher.PublishAsync(new ProbeEvent(), timeout: null);
        }

        var timedPublish = publisher.PublishAsync(new ProbeEvent(), timeout: null);
        await Assert.ThrowsAsync<TimeoutException>(() => timedPublish.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(timedPublish.IsFaulted);
    }

    /// <summary>单次超时遵循全局配置相同的有限正数边界，并拒绝空事件。</summary>
    /// <param name="milliseconds">超出支持范围的毫秒数。</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4294967295d)]
    public async Task InvalidPerPublishTimeoutIsRejected(double milliseconds)
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework(builder => builder.UseLocalEventPublisher());
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            publisher.PublishAsync(new ProbeEvent(), TimeSpan.FromMilliseconds(milliseconds)));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            publisher.PublishAsync<ProbeEvent>(null, TimeSpan.FromSeconds(1)));
    }

    /// <summary>负价格、负折扣、超额折扣及零数量均不能进入聚合。</summary>
    /// <param name="price">测试单价。</param>
    /// <param name="units">测试数量。</param>
    /// <param name="discount">测试折扣。</param>
    [Theory]
    [InlineData(-1, 1, -2)]
    [InlineData(10, 1, -1)]
    [InlineData(10, 1, 11)]
    [InlineData(10, 0, 0)]
    public void InvalidOrderItemIsRejected(int price, int units, int discount)
    {
        var order = CreateOrder();
        Assert.Throws<OrderingDomainException>(() => order.AddItem("product", "Product", null, price, units, discount));
        Assert.Empty(order.Items);
    }

    /// <summary>折扣修改、数量溢出与金额溢出失败后，订单项保持原状态。</summary>
    [Fact]
    public void InvalidMutationDoesNotPartiallyChangeItem()
    {
        var order = CreateOrder();
        var item = order.AddItem("product", "Product", null, 10);
        Assert.Throws<OrderingDomainException>(() => item.SetDiscount(11));
        Assert.Throws<OrderingDomainException>(() => item.AddUnits(int.MaxValue));
        Assert.Throws<OrderingDomainException>(() => item.AddUnits(0));
        Assert.Throws<OrderingDomainException>(() => order.AddItem("product", "Product", null, 10, 1, 21));
        Assert.Equal(1, item.Units);
        Assert.Equal(0, item.Discount);
        Assert.Throws<OrderingDomainException>(() => order.AddItem("large", "Large", null, decimal.MaxValue, 2));
        Assert.Single(order.Items);
    }

    /// <summary>合并新增数量后，允许折扣达到合并后的金额上界。</summary>
    [Fact]
    public void MergeValidatesDiscountAgainstCombinedQuantity()
    {
        var order = CreateOrder();
        order.AddItem("product", "Product", null, 10);
        var item = order.AddItem("product", "Product", null, 10, 1, 20);
        Assert.Equal(2, item.Units);
        Assert.Equal(20, item.Discount);
    }

    private static Order CreateOrder() => Order.Create(UserInfo.Create("buyer", "Buyer"), new Address(), "Test");

    /// <summary>只用于填充通道的不可变事件。</summary>
    public record ProbeEvent : EventBase;
}
