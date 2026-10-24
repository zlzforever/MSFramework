using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public sealed class DomainEventPersistenceTests
{
    private sealed record OrderSaved(int Sequence) : DomainEvent;

    private sealed class RecordingMediator
    {
        public RecordingMediator()
        {
            Instance = DispatchProxy.Create<IMediator, RecordingMediatorProxy>();
            ((RecordingMediatorProxy)(object)Instance).Owner = this;
        }

        public IMediator Instance { get; }
        public List<int> Received { get; } = [];
        public bool Throw { get; set; }

        private Task PublishAsync(Request message)
        {
            if (Throw)
            {
                throw new InvalidOperationException("handler failed");
            }

            Received.Add(((OrderSaved)message).Sequence);
            return Task.CompletedTask;
        }

        private class RecordingMediatorProxy : DispatchProxy
        {
            private static readonly MethodInfo FromResultMethod = typeof(Task)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(x => x.Name == nameof(Task.FromResult) && x.IsGenericMethodDefinition);

            public RecordingMediator Owner { get; set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (targetMethod.Name == nameof(IMediator.PublishAsync))
                {
                    return Owner.PublishAsync((Request)args[0]);
                }

                if (targetMethod.ReturnType == typeof(Task))
                {
                    return Task.CompletedTask;
                }

                var responseType = targetMethod.ReturnType.GenericTypeArguments[0];
                var defaultValue = responseType.IsValueType ? Activator.CreateInstance(responseType) : null;
                return FromResultMethod.MakeGenericMethod(responseType).Invoke(null, [defaultValue]);
            }
        }
    }

    private sealed class Order(string id) : EntityBase<string>(id)
    {
        public string Name { get; set; } = id;
    }

    private sealed class EventContext(DbContextOptions<EventContext> options) : DbContextBase(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().HasIndex(x => x.Name).IsUnique();
        }
    }

    private sealed class Finder : IEntityConfigurationTypeFinder
    {
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];
        public Type GetDbContextTypeForEntity(Type entityType) =>
            entityType == typeof(Order) ? typeof(EventContext) : null;
        public IEnumerable<Type> GetAllDbContextTypes() => [typeof(EventContext)];
        public bool HasDbContextForEntity<T>() => typeof(T) == typeof(Order);
    }

    private sealed class ScopeProvider(IServiceProvider provider) : IScopeServiceProvider
    {
        public T GetService<T>() => provider.GetService<T>();
    }

    [Fact]
    public async Task SaveChanges_PublishesEqualEventsInOriginalOrderAfterCommit()
    {
        await using var host = CreateHost();
        var context = host.Provider.GetRequiredService<EventContext>();
        var order = new Order("1");
        order.AddDomainEvent(new OrderSaved(1));
        order.AddDomainEvent(new OrderSaved(1));
        order.AddDomainEvent(new OrderSaved(2));
        context.Add(order);

        await context.SaveChangesAsync();

        Assert.Equal([1, 1, 2], host.Mediator.Received);
        Assert.Empty(order.GetDomainEvents());
        Assert.Equal(1, await context.Orders.CountAsync());
    }

    [Fact]
    public async Task SaveChanges_DatabaseFailureDoesNotPublishOrClearEvents()
    {
        await using var host = CreateHost();
        var context = host.Provider.GetRequiredService<EventContext>();
        var first = new Order("1") { Name = "duplicate" };
        var second = new Order("2") { Name = "duplicate" };
        first.AddDomainEvent(new OrderSaved(1));
        context.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Empty(host.Mediator.Received);
        Assert.Single(first.GetDomainEvents());
    }

    [Fact]
    public async Task SaveChanges_HandlerFailureRetainsEventForRetry()
    {
        await using var host = CreateHost();
        var context = host.Provider.GetRequiredService<EventContext>();
        var order = new Order("retry");
        order.AddDomainEvent(new OrderSaved(7));
        context.Add(order);
        host.Mediator.Throw = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Single(order.GetDomainEvents());
        Assert.Equal(1, await context.Orders.CountAsync());

        host.Mediator.Throw = false;
        await context.SaveChangesAsync();

        Assert.Equal([7], host.Mediator.Received);
        Assert.Empty(order.GetDomainEvents());
    }

    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var mediator = new RecordingMediator();
        var services = new ServiceCollection();
        services.AddMicroserviceFramework();
        services.AddSingleton(mediator.Instance);
        services.AddSingleton(new DbContextSettings { DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new Finder());
        services.AddDbContext<EventContext>(options => options.UseSqlite(connection));
        var root = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new ScopeProvider(root));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<EventContext>().Database.EnsureCreated();
        return new TestHost(provider, root, connection, mediator);
    }

    private sealed class TestHost(
        ServiceProvider provider,
        ServiceProvider root,
        SqliteConnection connection,
        RecordingMediator mediator) : IAsyncDisposable
    {
        public ServiceProvider Provider { get; } = provider;
        public RecordingMediator Mediator { get; } = mediator;

        public async ValueTask DisposeAsync()
        {
            await Provider.DisposeAsync();
            await root.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
