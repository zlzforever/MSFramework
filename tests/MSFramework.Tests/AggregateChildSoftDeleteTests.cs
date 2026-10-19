using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 聚合根集合移除子项的软删除回归测试：
/// 验证通过 <c>OnSoftDelete()</c> 配置（必需关系 + ClientCascade）后，
/// 从聚合根集合中移除 <see cref="IDeletion"/> 子项时，保存管道将其转换为软删除 UPDATE——
/// 外键列不置空、删除审计字段写入、未被业务修改的列不写库（避免全列写放大）、
/// "先修改值再删除"的业务变更不丢失。
/// </summary>
public class AggregateChildSoftDeleteTests
{
    /// <summary>
    /// 测试用聚合根：含私有子项集合
    /// </summary>
    public class SoftDeleteOrder : DeletionAggregateRoot<string>
    {
        private readonly List<SoftDeleteOrderItem> _items = new();

        /// <summary>
        /// 订单项
        /// </summary>
        public IReadOnlyCollection<SoftDeleteOrderItem> Items => _items;

        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected SoftDeleteOrder() : base(default!)
        {
        }

        /// <summary>
        /// 初始化聚合根
        /// </summary>
        /// <param name="id">主键</param>
        public SoftDeleteOrder(string id) : base(id)
        {
        }

        /// <summary>
        /// 添加订单项
        /// </summary>
        /// <param name="item">订单项</param>
        public void AddItem(SoftDeleteOrderItem item) => _items.Add(item);

        /// <summary>
        /// 从集合中移除订单项
        /// </summary>
        /// <param name="itemId">订单项标识</param>
        /// <returns>是否移除成功</returns>
        public bool DeleteItem(string itemId)
        {
            var item = _items.SingleOrDefault(x => x.Id == itemId);
            return _items.Remove(item);
        }
    }

    /// <summary>
    /// 测试用软删除子实体
    /// </summary>
    public class SoftDeleteOrderItem : DeletionEntity<string>
    {
        /// <summary>
        /// 所属订单
        /// </summary>
        public SoftDeleteOrder Order { get; private set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; private set; }

        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected SoftDeleteOrderItem() : base(default!)
        {
        }

        /// <summary>
        /// 初始化子实体
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="productName">产品名称</param>
        public SoftDeleteOrderItem(string id, string productName) : base(id)
        {
            ProductName = productName;
        }

        /// <summary>
        /// 修改产品名称
        /// </summary>
        /// <param name="productName">新产品名称</param>
        public void ChangeProductName(string productName) => ProductName = productName;
    }

    /// <summary>
    /// 测试用 DbContext：关系通过 <c>OnSoftDelete()</c> 配置（必需 + ClientCascade）
    /// </summary>
    public class AggregateChildContext(DbContextOptions<AggregateChildContext> options) : DbContextBase(options)
    {
        /// <summary>
        /// 订单集合
        /// </summary>
        public DbSet<SoftDeleteOrder> Orders => Set<SoftDeleteOrder>();

        /// <summary>
        /// 订单项集合
        /// </summary>
        public DbSet<SoftDeleteOrderItem> OrderItems => Set<SoftDeleteOrderItem>();

        /// <inheritdoc />
        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SoftDeleteOrder>(builder =>
            {
                builder.Property(x => x.Id).HasMaxLength(36);
                builder.HasMany(x => x.Items).WithOne(x => x.Order).OnSoftDelete();
            });
            modelBuilder.Entity<SoftDeleteOrderItem>(builder =>
            {
                builder.Property(x => x.Id).HasMaxLength(36);
                builder.Property(x => x.ProductName).HasMaxLength(100);
            });
        }
    }

    /// <summary>
    /// UPDATE 语句记录拦截器
    /// </summary>
    private sealed class UpdateRecorder : DbCommandInterceptor
    {
        /// <summary>
        /// 已执行的 UPDATE 语句
        /// </summary>
        public List<string> UpdateCommands { get; } = [];

        /// <summary>
        /// 清空记录
        /// </summary>
        public void Reset() => UpdateCommands.Clear();

        private void Record(DbCommand command)
        {
            if (command.CommandText.TrimStart().StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCommands.Add(command.CommandText);
            }
        }

        /// <inheritdoc />
        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result)
        {
            Record(command);
            return result;
        }

        /// <inheritdoc />
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Record(command);
            return new ValueTask<InterceptionResult<int>>(result);
        }

        /// <inheritdoc />
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            Record(command);
            return result;
        }

        /// <inheritdoc />
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Record(command);
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }
    }

    /// <summary>
    /// 测试用实体配置查找器
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(SoftDeleteOrder)] = typeof(AggregateChildContext),
            [typeof(SoftDeleteOrderItem)] = typeof(AggregateChildContext)
        };

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() => EntityToDbContext.Values.Distinct();

        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 测试用会话
    /// </summary>
    private sealed class TestSession : ISession
    {
        public string TraceIdentifier => "aggregate-child-test";

        public string UserId => "u1";

        public string UserName => "user-1";

        public string Email => null;

        public string PhoneNumber => null;

        public string UserDisplayName => "user-1";

        public IReadOnlyCollection<string> Roles => [];

        public IReadOnlyCollection<string> Subjects => [];

        public string GetValue(SessionField field) => null;

        public void Load(ISession session)
        {
        }
    }

    /// <summary>
    /// 测试用作用域服务提供程序
    /// </summary>
    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接与服务提供程序
    /// </summary>
    private sealed class TestHost(ServiceProvider provider, SqliteConnection connection) : IDisposable
    {
        public ServiceProvider Provider { get; } = provider;

        public SqliteConnection Connection { get; } = connection;

        public void Dispose()
        {
            Connection.Dispose();
            Provider.Dispose();
        }
    }

    private static TestHost CreateHost(UpdateRecorder recorder)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddScoped<ISession>(_ => new TestSession());
        services.AddDbContext<AggregateChildContext>(options =>
            options.UseSqlite(connection).AddInterceptors(recorder));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<AggregateChildContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 提取 UPDATE 语句 SET 子句的列名集合（去引号、小写）
    /// </summary>
    /// <param name="updateSql">UPDATE 语句原文</param>
    /// <returns>SET 子句列名集合</returns>
    private static HashSet<string> ExtractSetColumns(string updateSql)
    {
        var sql = updateSql.ToLowerInvariant();
        var setStart = sql.IndexOf("set ", StringComparison.Ordinal);
        var whereStart = sql.IndexOf("where ", StringComparison.Ordinal);
        var setClause = sql.Substring(setStart + 4, whereStart - setStart - 4);
        return setClause.Split(',', StringSplitOptions.TrimEntries)
            .Select(column => column.Split('=')[0].Trim().Trim('"'))
            .ToHashSet();
    }

    /// <summary>
    /// 从聚合根集合移除子项：行被软删除、外键保留、UPDATE 仅含删除审计列
    /// </summary>
    [Fact]
    public async Task RemoveChild_SoftDeletesRow_KeepsForeignKey_OnlyAuditColumns()
    {
        var recorder = new UpdateRecorder();
        using var host = CreateHost(recorder);

        string orderId;
        string removedItemId;
        string keptItemId;
        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = new SoftDeleteOrder(Guid.NewGuid().ToString("N"));
            var removed = new SoftDeleteOrderItem(Guid.NewGuid().ToString("N"), "removed");
            var kept = new SoftDeleteOrderItem(Guid.NewGuid().ToString("N"), "kept");
            order.AddItem(removed);
            order.AddItem(kept);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
            removedItemId = removed.Id;
            keptItemId = kept.Id;
        }

        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = await context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == orderId);
            Assert.True(order.DeleteItem(removedItemId));

            recorder.Reset();
            await context.SaveChangesAsync();

            // 软删除 UPDATE 仅包含四个删除审计列，外键列（未变化）与业务列（未修改）不写库
            var update = Assert.Single(recorder.UpdateCommands);
            Assert.Equal(
                new HashSet<string> { "is_deleted", "deleter_id", "deleter_name", "deletion_time" },
                ExtractSetColumns(update));
        }

        using var verifyScope = host.Provider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AggregateChildContext>();

        // 默认查询（带软删除过滤器）查不到已删除的子项
        Assert.Null(await verifyContext.OrderItems.SingleOrDefaultAsync(x => x.Id == removedItemId));
        // 其余子项不受影响
        Assert.NotNull(await verifyContext.OrderItems.SingleOrDefaultAsync(x => x.Id == keptItemId));

        // 物理行仍存在：外键保留（父子对应关系不丢失），删除审计字段来自会话
        var row = await verifyContext.OrderItems.IgnoreQueryFilters()
            .Where(x => x.Id == removedItemId)
            .Select(x => new
            {
                OrderId = EF.Property<string>(x, "OrderId"),
                x.IsDeleted,
                x.DeleterId,
                x.DeleterName
            })
            .SingleAsync();
        Assert.Equal(orderId, row.OrderId);
        Assert.True(row.IsDeleted);
        Assert.Equal("u1", row.DeleterId);
        Assert.Equal("user-1", row.DeleterName);
    }

    /// <summary>
    /// 软删除保存后同一上下文再次保存（如领域事件处理器触发第二次 SaveChanges）：
    /// 已收回为软删除的子项不应被 EF 重新标记孤儿，第二次保存不产生任何 UPDATE
    /// </summary>
    [Fact]
    public async Task RemoveChild_SecondSave_ProducesNoFurtherUpdate()
    {
        var recorder = new UpdateRecorder();
        using var host = CreateHost(recorder);

        string orderId;
        string itemId;
        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = new SoftDeleteOrder(Guid.NewGuid().ToString("N"));
            var item = new SoftDeleteOrderItem(Guid.NewGuid().ToString("N"), "item");
            order.AddItem(item);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
            itemId = item.Id;
        }

        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = await context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == orderId);
            Assert.True(order.DeleteItem(itemId));

            await context.SaveChangesAsync();
            Assert.Equal(1, recorder.UpdateCommands.Count);

            // 第二次保存：不应把已软删除的子项重新孤儿化（不产生新 UPDATE，更不能物理删除）
            recorder.Reset();
            await context.SaveChangesAsync();
            Assert.Empty(recorder.UpdateCommands);
        }

        using var verifyScope = host.Provider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AggregateChildContext>();
        var row = await verifyContext.OrderItems.IgnoreQueryFilters().SingleAsync(x => x.Id == itemId);
        Assert.True(row.IsDeleted);
    }

    /// <summary>
    /// 先修改子项业务字段再从集合移除：业务修改与删除审计列一起落库，不丢失
    /// </summary>
    [Fact]
    public async Task RemoveChild_KeepsBusinessModifications_MadeBeforeDelete()    {
        var recorder = new UpdateRecorder();
        using var host = CreateHost(recorder);

        string orderId;
        string itemId;
        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = new SoftDeleteOrder(Guid.NewGuid().ToString("N"));
            var item = new SoftDeleteOrderItem(Guid.NewGuid().ToString("N"), "old-name");
            order.AddItem(item);
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
            itemId = item.Id;
        }

        using (var scope = host.Provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AggregateChildContext>();
            var order = await context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == orderId);
            var item = order.Items.Single(x => x.Id == itemId);
            item.ChangeProductName("new-name");
            Assert.True(order.DeleteItem(itemId));

            recorder.Reset();
            await context.SaveChangesAsync();

            // UPDATE 包含删除审计列 + 被业务真改过的列
            var update = Assert.Single(recorder.UpdateCommands);
            Assert.Equal(
                new HashSet<string> { "is_deleted", "deleter_id", "deleter_name", "deletion_time", "product_name" },
                ExtractSetColumns(update));
        }

        using var verifyScope = host.Provider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AggregateChildContext>();
        var row = await verifyContext.OrderItems.IgnoreQueryFilters().SingleAsync(x => x.Id == itemId);
        Assert.True(row.IsDeleted);
        Assert.Equal("new-name", row.ProductName);
    }
}
