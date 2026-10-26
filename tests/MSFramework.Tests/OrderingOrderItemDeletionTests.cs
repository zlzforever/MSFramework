using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Commands;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Domain.Repositories.Order;
using Ordering.Infrastructure;
using Ordering.Infrastructure.EntityConfigurations;
using Ordering.Infrastructure.Repositories.Order;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.Tests;

/// <summary>
/// Sample Ordering 订单项删除回归测试。
/// </summary>
public class OrderingOrderItemDeletionTests
{
    private readonly ITestOutputHelper _output;

    public OrderingOrderItemDeletionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DeleteOrderItem_SoftDeletesRow_AndKeepsOrderForeignKey()
    {
        using var host = CreateHost();
        var (orderId, orderItemId) = await SeedOrderAsync(host);

        using (var scope = host.Provider.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<DeleteOrderItemCommandHandler>();
            var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();

            await handler.HandleAsync(new DeleteOrderItemCommand
            {
                OrderId = orderId,
                OrderItemId = orderItemId
            }, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        using var verificationScope = host.Provider.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<OrderingContext>();

        var visibleItem = await verificationContext.Set<OrderItem>()
            .SingleOrDefaultAsync(x => x.Id == orderItemId);
        Assert.Null(visibleItem);

        var physicalRow = await ReadOrderItemRowAsync(verificationContext, orderItemId);
        Assert.Equal(orderId, physicalRow.OrderId);
        Assert.True(physicalRow.IsDeleted);

        var foreignKey = verificationContext.Model.FindEntityType(typeof(OrderItem))!
            .GetForeignKeys()
            .Single(x => x.PrincipalEntityType.ClrType == typeof(Order));
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.ClientCascade, foreignKey.DeleteBehavior);

        WriteDatabaseInspectionHint(host.Settings, orderId, orderItemId);
    }

    [Fact]
    public async Task DeleteOrderItem_WhenItemBelongsToAnotherOrder_DoesNothing()
    {
        using var host = CreateHost();
        var (orderId, orderItemId) = await SeedOrderAsync(host);
        var otherOrder = await SeedOrderAsync(host);

        using (var scope = host.Provider.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<DeleteOrderItemCommandHandler>();
            var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();

            await handler.HandleAsync(new DeleteOrderItemCommand
            {
                OrderId = otherOrder.OrderId,
                OrderItemId = orderItemId
            }, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        using var verificationScope = host.Provider.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<OrderingContext>();
        var physicalRow = await ReadOrderItemRowAsync(verificationContext, orderItemId);

        Assert.Equal(orderId, physicalRow.OrderId);
        Assert.False(physicalRow.IsDeleted);
    }

    private static TestHost CreateHost()
    {
        var settings = LoadApiDatabaseSettings();
        // 默认测试不读取仓库凭据、不连接外部服务；显式注入连接时保留 Provider 集成验证。
        var connection = settings.DatabaseType == "SQLite"
            ? new SqliteConnection("Data Source=:memory:;Foreign Keys=False")
            : null;
        connection?.Open();

        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddSingleton<IEntityConfigurationTypeFinder>(new OrderingEntityConfigurationTypeFinder());
        services.AddScoped<ISession, TestSession>();
        services.AddDbContext<OrderingContext>(options =>
        {
            if (connection != null)
            {
                options.UseSqlite(connection);
                ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(settings);
            }
            else
            {
                ConfigureDatabase(options, settings);
            }
        });
        services.AddEntityFrameworkExtension();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<DeleteOrderItemCommandHandler>();
        services.AddScoped<IScopeServiceProvider>(provider => new TestScopeServiceProvider(provider));

        var provider = services.BuildServiceProvider();

        if (connection != null)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();
            context.Database.EnsureCreated();
            // 外部用户表被框架排除迁移，由测试模拟其拥有服务创建。
            context.Database.ExecuteSqlRaw("CREATE TABLE user (id TEXT PRIMARY KEY, name TEXT NOT NULL)");
        }

        return new TestHost(provider, settings, connection);
    }

    private static async Task<(string OrderId, string OrderItemId)> SeedOrderAsync(
        TestHost host,
        string userId = null)
    {
        userId ??= Guid.NewGuid().ToString("N");

        using var scope = host.Provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();

        if (host.Settings.DatabaseType == "SQLite")
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO user (id, name) VALUES ({userId}, {"Test User"})");
        }

        var order = Order.Create(
            UserInfo.Create(userId, "Test User"),
            new Address
            {
                City = "City",
                Country = "Country",
                State = "State",
                Street = "Street",
                ZipCode = "ZipCode"
            },
            "Description");
        var targetItem = order.AddItem("product-1", "Product 1", null, 100);
        order.AddItem("product-2", "Product 2", null, 200);

        context.Set<Order>().Add(order);
        await context.SaveChangesAsync();
        return (order.Id, targetItem.Id);
    }

    private static async Task<(string OrderId, bool IsDeleted)> ReadOrderItemRowAsync(
        OrderingContext context,
        string orderItemId)
    {
        var row = await context.Set<OrderItem>()
            .IgnoreQueryFilters()
            .Where(x => x.Id == orderItemId)
            .Select(x => new
            {
                OrderId = EF.Property<string>(x, "OrderId"),
                x.IsDeleted
            })
            .SingleOrDefaultAsync();

        Assert.NotNull(row);
        return (row.OrderId, row.IsDeleted);
    }

    private static DbContextSettings LoadApiDatabaseSettings()
    {
        var apiDirectory = FindApiProjectDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var settings = configuration.GetDbContextSettings<OrderingContext>();
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            settings.DatabaseType = "SQLite";
            settings.ConnectionString = "Data Source=:memory:";
        }

        return settings;
    }

    private static string FindApiProjectDirectory()
    {
        var startDirectories = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var startDirectory in startDirectories)
        {
            for (var directory = startDirectory; directory != null; directory = directory.Parent)
            {
                var apiDirectory = Path.Combine(
                    directory.FullName,
                    "src",
                    "Sample",
                    "Ordering.API");
                if (File.Exists(Path.Combine(apiDirectory, "appsettings.Development.json")))
                {
                    return apiDirectory;
                }
            }
        }

        throw new DirectoryNotFoundException(
            "未找到 src/Sample/Ordering.API/appsettings.Development.json。请从 MSFramework 仓库运行测试。");
    }

    private static void ConfigureDatabase(
        DbContextOptionsBuilder options,
        DbContextSettings settings)
    {
        if ("mysql".Equals(settings.DatabaseType, StringComparison.OrdinalIgnoreCase))
        {
            options.UseMySql(
                ServerVersion.AutoDetect(settings.ConnectionString),
                mysql => mysql.Load(settings));
            return;
        }

        if ("postgresql".Equals(settings.DatabaseType, StringComparison.OrdinalIgnoreCase) ||
            "postgres".Equals(settings.DatabaseType, StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(postgres => postgres.Load(settings));
            return;
        }

        throw new NotSupportedException(
            $"Ordering.API 测试暂不支持数据库类型: {settings.DatabaseType}");
    }

    private void WriteDatabaseInspectionHint(
        DbContextSettings settings,
        string orderId,
        string orderItemId)
    {
        _output.WriteLine($"DatabaseType: {settings.DatabaseType}");
        _output.WriteLine($"OrderId: {orderId}");
        _output.WriteLine($"OrderItemId: {orderItemId}");
        _output.WriteLine(
            $"SQL: SELECT id, order_id, is_deleted, deletion_time FROM {settings.TablePrefix}order_item WHERE id = '{orderItemId}';");
    }

    private sealed class TestHost(ServiceProvider provider, DbContextSettings settings, SqliteConnection connection) : IDisposable
    {
        public ServiceProvider Provider { get; } = provider;
        public DbContextSettings Settings { get; } = settings;

        public void Dispose()
        {
            Provider.Dispose();
            connection?.Dispose();
        }
    }

    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    private sealed class TestSession : ISession
    {
        public string TraceIdentifier => "ordering-order-item-test";

        public string UserId => "deleter-1";

        public string UserName => "Deleter";

        public string Email => null;

        public string PhoneNumber => null;

        public string UserDisplayName => "Deleter";

        public IReadOnlyCollection<string> Roles => [];

        public IReadOnlyCollection<string> Subjects => [];

        public string GetValue(SessionField field) => null;

        public void Load(ISession session)
        {
        }
    }

    private sealed class OrderingEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly MicroserviceFramework.Ef.IEntityTypeConfiguration[] Configurations =
        [
            new OrderConfiguration(),
            new OrderItemConfiguration(),
            new UserConfiguration()
        ];

        public IEnumerable<MicroserviceFramework.Ef.IEntityTypeConfiguration> GetEntityTypeConfigurations(
            Type dbContextType)
        {
            return dbContextType == typeof(OrderingContext) ? Configurations : [];
        }

        public Type GetDbContextTypeForEntity(Type entityType)
        {
            return entityType == typeof(Order) ||
                   entityType == typeof(OrderItem) ||
                   entityType == typeof(UserInfo)
                ? typeof(OrderingContext)
                : null;
        }

        public IEnumerable<Type> GetAllDbContextTypes() => [typeof(OrderingContext)];

        public bool HasDbContextForEntity<T>() => GetDbContextTypeForEntity(typeof(T)) != null;
    }
}
