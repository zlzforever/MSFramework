using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Commands;
using Ordering.Domain;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Domain.Repositories.Order;
using Xunit;
using CatalogProduct = Ordering.Domain.AggregateRoots.Product;

namespace MSFramework.Tests;

/// <summary>下单身份与价格边界测试，使用内存商品目录和记录型仓储，不依赖外部服务。</summary>
public class CreateOrderSecurityTests
{
    /// <summary>伪造的身份及金额字段不进入命令契约，订单使用会话和目录值。</summary>
    [Fact]
    public async Task ClientCannotChooseBuyerPriceOrDiscount()
    {
        await using var context = CreateContext();
        var product = CatalogProduct.CreateWithoutEvent("Catalog name", 1234);
        context.Add(product);
        await context.SaveChangesAsync();
        var command = JsonSerializer.Deserialize<CreateOrderCommand>(JsonSerializer.Serialize(new
        {
            UserId = "attacker-chosen", OrderItems = new[]
            {
                new { ProductId = product.Id, Units = 2, UnitPrice = 1, Discount = 999, ProductName = "Forged" }
            }
        }));
        var repository = new RecordingRepository();
        var handler = new CreateOrderCommandHandler(repository, CreateSession("authenticated-buyer"), new ContextFactory(context));
        await handler.HandleAsync(command, CancellationToken.None);
        Assert.Equal("authenticated-buyer", repository.Added.Buyer.Id);
        var item = Assert.Single(repository.Added.Items);
        Assert.Equal(1234, item.Price);
        Assert.Equal(0, item.Discount);
        Assert.Equal("Catalog name", item.Product.Name);
    }

    /// <summary>缺少认证用户时不得读取目录或加入订单仓储。</summary>
    [Fact]
    public async Task AnonymousBuyerIsRejected()
    {
        await using var context = CreateContext();
        var repository = new RecordingRepository();
        var handler = new CreateOrderCommandHandler(repository, CreateSession(null), new ContextFactory(context));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(new CreateOrderCommand(), CancellationToken.None));
        Assert.Null(repository.Added);
    }

    /// <summary>不存在的目录商品不能形成订单。</summary>
    [Fact]
    public async Task UnknownCatalogProductIsRejected()
    {
        await using var context = CreateContext();
        var repository = new RecordingRepository();
        var handler = new CreateOrderCommandHandler(repository, CreateSession("buyer"), new ContextFactory(context));
        await Assert.ThrowsAsync<OrderingDomainException>(() => handler.HandleAsync(new CreateOrderCommand
        {
            OrderItems = [new() { ProductId = "unknown", Units = 1 }]
        }, CancellationToken.None));
        Assert.Null(repository.Added);
    }

    private static CatalogContext CreateContext() => new(new DbContextOptionsBuilder<CatalogContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static HttpSession CreateSession(string userId)
    {
        var context = new DefaultHttpContext();
        if (userId != null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId)], "test"));
        }
        return HttpSession.Create(new HttpContextAccessor { HttpContext = context });
    }

    private sealed class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options)
    {
        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<CatalogProduct>();
    }

    private sealed class ContextFactory(DbContext context) : Ordering.Application.IDbContextFactory
    {
        /// <inheritdoc/>
        public DbContext GetDbContext<TEntity>() => context;
    }

    private sealed class RecordingRepository : IOrderRepository
    {
        /// <summary>处理器最近一次加入的订单，未调用时为空。</summary>
        public Order Added { get; private set; }

        /// <inheritdoc/>
        public void Add(Order entity) => Added = entity;
        /// <inheritdoc/>
        public Task AddAsync(Order entity) { Add(entity); return Task.CompletedTask; }
        /// <inheritdoc/>
        public void Delete(Order entity) => throw new NotSupportedException();
        /// <inheritdoc/>
        public Task DeleteAsync(Order entity) => throw new NotSupportedException();
        /// <inheritdoc/>
        public Order Find(string id) => throw new NotSupportedException();
        /// <inheritdoc/>
        public Task<Order> FindAsync(string id) => throw new NotSupportedException();
        /// <inheritdoc/>
        public void Delete(string id) => throw new NotSupportedException();
        /// <inheritdoc/>
        public Task DeleteAsync(string id) => throw new NotSupportedException();
        /// <inheritdoc/>
        public void B() => throw new NotSupportedException();
    }
}
