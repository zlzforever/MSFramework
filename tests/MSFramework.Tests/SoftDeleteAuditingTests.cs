using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 软删除审计回归测试（ZZO-334）：
/// 验证 <see cref="DeletionAuditingStrategy"/> 去掉 Reload 后的行为——
/// 批量删除不再产生逐行 SELECT、软删除 UPDATE 仅包含删除审计列、
/// 非审计列数据不被清空、乐观锁语义保留（WHERE 按原令牌判断且不重写令牌）、
/// stub 删除（仅构造主键、未加载）不会把空值写入数据库、
/// stub + 乐观锁组合删除抛 <see cref="DbUpdateConcurrencyException"/> 且行未被删除。
/// </summary>
public class SoftDeleteAuditingTests
{
    /// <summary>
    /// 无乐观锁的软删除聚合根
    /// </summary>
    public class SoftDeleteCategory : DeletionAggregateRoot<string>
    {
        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected SoftDeleteCategory() : base(default!)
        {
        }

        /// <summary>
        /// 初始化软删除聚合根
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="name">业务名称</param>
        public SoftDeleteCategory(string id, string name) : base(id)
        {
            Name = name;
        }

        /// <summary>
        /// 业务名称列，用于验证软删除不写入该列
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 带乐观锁（<see cref="IOptimisticLock.ConcurrencyStamp"/>）的软删除聚合根
    /// </summary>
    public class VersionedCategory : DeletionAggregateRoot<string>, IOptimisticLock
    {
        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected VersionedCategory() : base(default!)
        {
        }

        /// <summary>
        /// 初始化带乐观锁的软删除聚合根
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="name">业务名称</param>
        public VersionedCategory(string id, string name) : base(id)
        {
            Name = name;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 业务名称列
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 乐观锁令牌
        /// </summary>
        public string ConcurrencyStamp { get; set; }
    }

    /// <summary>
    /// 测试用 DbContext
    /// </summary>
    public class SoftDeleteAuditingContext(DbContextOptions<SoftDeleteAuditingContext> options) : DbContextBase(options)
    {
        /// <summary>
        /// 无乐观锁软删除实体集合
        /// </summary>
        public DbSet<SoftDeleteCategory> Categories => Set<SoftDeleteCategory>();

        /// <summary>
        /// 带乐观锁软删除实体集合
        /// </summary>
        public DbSet<VersionedCategory> VersionedCategories => Set<VersionedCategory>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SoftDeleteCategory>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
            modelBuilder.Entity<VersionedCategory>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 命令计数拦截器：统计执行期间产生的 SELECT / UPDATE 语句并记录 UPDATE 文本，用于 SQL 断言
    /// </summary>
    private sealed class CountingCommandInterceptor : DbCommandInterceptor
    {
        /// <summary>
        /// 累计 SELECT 条数
        /// </summary>
        public int SelectCount { get; private set; }

        /// <summary>
        /// 累计 UPDATE 条数
        /// </summary>
        public int UpdateCount { get; private set; }

        /// <summary>
        /// 已执行的 UPDATE 语句文本集合
        /// </summary>
        public List<string> UpdateCommands { get; } = [];

        /// <summary>
        /// 累计命令总数（任意类型），用于探针验证
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// 清零计数器，供分阶段断言（种子写入完成后清零，再统计被测保存阶段）
        /// </summary>
        public void Reset()
        {
            TotalCount = 0;
            SelectCount = 0;
            UpdateCount = 0;
            UpdateCommands.Clear();
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            TotalCount++;
            CountCommand(command);
            return result;
        }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result)
        {
            TotalCount++;
            CountCommand(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            TotalCount++;
            CountCommand(command);
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            TotalCount++;
            CountCommand(command);
            return new ValueTask<InterceptionResult<int>>(result);
        }

        /// <summary>
        /// 按命令类型分类计数：UPDATE 计数并记录文本，SELECT 计数
        /// （SQLite 提供商的 UPDATE 可能经 Reader 路径执行，因此两类执行路径统一识别）
        /// </summary>
        /// <param name="command">已执行命令</param>
        private void CountCommand(DbCommand command)
        {
            var text = command.CommandText.TrimStart();
            if (text.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                SelectCount++;
            }
            else if (text.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCount++;
                UpdateCommands.Add(command.CommandText);
            }
        }
    }

    /// <summary>
    /// 测试用会话，提供固定用户标识与显示名称，供删除审计字段写入断言
    /// </summary>
    private sealed class TestSession(string userId, string userName) : ISession
    {
        /// <summary>
        /// 跟踪标识
        /// </summary>
        public string TraceIdentifier => "trace-1";

        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserId => userId;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName => userName;

        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string Email => null;

        /// <summary>
        /// 用户电话
        /// </summary>
        public string PhoneNumber => null;

        /// <summary>
        /// 用户显示名称
        /// </summary>
        public string UserDisplayName => userName;

        /// <summary>
        /// 用户角色
        /// </summary>
        public IReadOnlyCollection<string> Roles => [];

        /// <summary>
        /// 用户主体
        /// </summary>
        public IReadOnlyCollection<string> Subjects => [];

        /// <summary>
        /// 提取请求头字段，测试场景固定返回 null
        /// </summary>
        /// <param name="field">字段定义</param>
        /// <returns>null</returns>
        public string GetValue(SessionField field) => null;

        /// <summary>
        /// 覆盖用户信息，测试场景无操作
        /// </summary>
        /// <param name="session">新会话</param>
        public void Load(ISession session)
        {
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(SoftDeleteCategory)] = typeof(SoftDeleteAuditingContext),
            [typeof(VersionedCategory)] = typeof(SoftDeleteAuditingContext)
        };

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() =>
            EntityToDbContext.Values.Distinct();

        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 测试用作用域服务提供程序
    /// </summary>
    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接、服务容器与命令计数拦截器，保证连接在测试生命周期内存活
    /// </summary>
    private sealed class TestHost : IDisposable
    {
        public TestHost(ServiceProvider provider, SqliteConnection connection, CountingCommandInterceptor interceptor)
        {
            Provider = provider;
            _connection = connection;
            Interceptor = interceptor;
        }

        /// <summary>
        /// 服务提供程序
        /// </summary>
        public ServiceProvider Provider { get; }

        /// <summary>
        /// 命令计数拦截器
        /// </summary>
        public CountingCommandInterceptor Interceptor { get; }

        private readonly SqliteConnection _connection;

        public void Dispose()
        {
            _connection.Dispose();
            Provider.Dispose();
        }
    }

    /// <summary>
    /// 测试用 DbContext 作用域：作用域随测试块释放，保证每个上下文实例独立
    /// </summary>
    private sealed class ContextScope : IDisposable
    {
        public ContextScope(IServiceScope scope, SoftDeleteAuditingContext context)
        {
            Scope = scope;
            Context = context;
        }

        /// <summary>
        /// 承载上下文的作用域
        /// </summary>
        public IServiceScope Scope { get; }

        /// <summary>
        /// 作用域内解析的 DbContext 实例
        /// </summary>
        public SoftDeleteAuditingContext Context { get; }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }

    /// <summary>
    /// 创建独立的测试 DbContext 作用域
    /// </summary>
    /// <param name="host">测试宿主</param>
    /// <returns>持有独立上下文实例的作用域</returns>
    private static ContextScope CreateContext(TestHost host)
    {
        var scope = host.Provider.CreateScope();
        return new ContextScope(scope, scope.ServiceProvider.GetRequiredService<SoftDeleteAuditingContext>());
    }

    /// <summary>
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库 + 命令计数拦截器）、
    /// 设置、实体查找器、作用域提供程序与测试会话
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var interceptor = new CountingCommandInterceptor();
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddScoped<ISession>(_ => new TestSession("u1", "user-1"));
        services.AddDbContext<SoftDeleteAuditingContext>(options =>
            options.UseSqlite(connection).AddInterceptors(interceptor));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<SoftDeleteAuditingContext>().Database.EnsureCreated();

        return new TestHost(provider, connection, interceptor);
    }

    /// <summary>
    /// 提取 UPDATE 语句 SET 子句的列名集合（去引号、小写），用于精确断言软删除写入的列
    /// </summary>
    /// <param name="updateSql">UPDATE 语句原文</param>
    /// <returns>SET 子句列名集合</returns>
    private static HashSet<string> ExtractSetColumns(string updateSql)
    {
        var sql = updateSql.ToLowerInvariant();
        var setStart = sql.IndexOf("set ", StringComparison.Ordinal);
        // SQLite 生成的 UPDATE 在 WHERE 前换行，因此按 "where " 定位即可
        var whereStart = sql.IndexOf("where ", StringComparison.Ordinal);
        var setClause = sql.Substring(setStart + 4, whereStart - setStart - 4);
        return setClause.Split(',', StringSplitOptions.TrimEntries)
            .Select(column => column.Split('=')[0].Trim().Trim('"'))
            .ToHashSet();
    }

    /// <summary>
    /// 批量软删除：保存阶段不再产生逐行 SELECT，N 行删除对应 N 条 UPDATE
    /// </summary>
    [Fact]
    public async Task BatchSoftDelete_NoPerRowSelect_OneUpdatePerEntity()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            for (var i = 0; i < 3; i++)
            {
                seedScope.Context.Categories.Add(new SoftDeleteCategory($"C-{i}", $"name-{i}"));
            }

            await seedScope.Context.SaveChangesAsync();
        }

        using var scope = CreateContext(host);
        var entities = scope.Context.Categories.ToList();
        foreach (var entity in entities)
        {
            scope.Context.Categories.Remove(entity);
        }

        host.Interceptor.Reset();
        await scope.Context.SaveChangesAsync();

        // 原 Reload 实现每行删除产生 1 条 SELECT（N+1），改造后保存阶段不允许出现 SELECT
        Assert.Equal(0, host.Interceptor.SelectCount);
        // 软删除转 Modified 后每行生成 1 条 UPDATE
        Assert.Equal(3, host.Interceptor.UpdateCount);
    }

    /// <summary>
    /// 软删除 UPDATE 的 SET 子句仅包含四个删除审计列，不包含业务列与其它审计列（防全列写放大）
    /// </summary>
    [Fact]
    public async Task SoftDelete_UpdateStatement_SetsOnlyAuditColumns()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            seedScope.Context.Categories.Add(new SoftDeleteCategory("C-1", "apple"));
            await seedScope.Context.SaveChangesAsync();
        }

        using var scope = CreateContext(host);
        var entity = scope.Context.Categories.Single(x => x.Id == "C-1");
        scope.Context.Categories.Remove(entity);

        host.Interceptor.Reset();
        await scope.Context.SaveChangesAsync();

        var update = Assert.Single(host.Interceptor.UpdateCommands);
        var setColumns = ExtractSetColumns(update);
        Assert.Equal(new HashSet<string> { "is_deleted", "deleter_id", "deleter_name", "deletion_time" }, setColumns);
    }

    /// <summary>
    /// 软删除后数据完整：行仍存在、审计字段写入、非审计列未被清空
    /// </summary>
    [Fact]
    public async Task SoftDelete_PreservesBusinessData_AndWritesAuditFields()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            seedScope.Context.Categories.Add(new SoftDeleteCategory("C-1", "apple"));
            await seedScope.Context.SaveChangesAsync();
        }

        using (var scope = CreateContext(host))
        {
            var entity = scope.Context.Categories.Single(x => x.Id == "C-1");
            scope.Context.Categories.Remove(entity);
            await scope.Context.SaveChangesAsync();
        }

        using var verifyScope = CreateContext(host);
        // IgnoreQueryFilters 绕过软删除全局过滤器，验证行仍存在且数据完整
        var persisted = verifyScope.Context.Categories.IgnoreQueryFilters().Single(x => x.Id == "C-1");
        Assert.True(persisted.IsDeleted);
        Assert.Equal("u1", persisted.DeleterId);
        Assert.Equal("user-1", persisted.DeleterName);
        Assert.NotNull(persisted.DeletionTime);
        Assert.Equal("apple", persisted.Name);
    }

    /// <summary>
    /// 乐观锁实体软删除：ConcurrencyStamp 不进 SET、以原值参与 WHERE 并发判断，且不重写令牌
    /// </summary>
    [Fact]
    public async Task SoftDelete_OptimisticLock_StampOnlyInWhere_AndUnchanged()
    {
        using var host = CreateHost();

        string stamp;
        using (var seedScope = CreateContext(host))
        {
            var category = new VersionedCategory("V-1", "apple");
            stamp = category.ConcurrencyStamp;
            seedScope.Context.VersionedCategories.Add(category);
            await seedScope.Context.SaveChangesAsync();
        }

        using var scope = CreateContext(host);
        var entity = scope.Context.VersionedCategories.Single(x => x.Id == "V-1");
        Assert.Equal(stamp, entity.ConcurrencyStamp);
        scope.Context.VersionedCategories.Remove(entity);

        host.Interceptor.Reset();
        await scope.Context.SaveChangesAsync();

        var update = Assert.Single(host.Interceptor.UpdateCommands).ToLowerInvariant();
        // 乐观锁列仅出现一次：只在 WHERE 中按原值参与判断，不出现在 SET 中
        Assert.Equal(1, CountOccurrences(update, "concurrency_stamp"));

        using var verifyScope = CreateContext(host);
        var persisted = verifyScope.Context.VersionedCategories.IgnoreQueryFilters().Single(x => x.Id == "V-1");
        Assert.True(persisted.IsDeleted);
        // 软删除不重写乐观锁令牌，令牌保持原值
        Assert.Equal(stamp, persisted.ConcurrencyStamp);
    }

    /// <summary>
    /// 乐观锁实体软删除：令牌已被其它会话更新时，提交应抛 DbUpdateConcurrencyException
    /// （WHERE 使用原始令牌匹配不到行，语义与修改路径一致）
    /// </summary>
    [Fact]
    public async Task SoftDelete_StaleEntity_ThrowsDbUpdateConcurrencyException()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            seedScope.Context.VersionedCategories.Add(new VersionedCategory("V-1", "apple"));
            await seedScope.Context.SaveChangesAsync();
        }

        // 上下文 A 加载实体准备删除，持有旧令牌
        using var staleScope = CreateContext(host);
        var stale = staleScope.Context.VersionedCategories.Single(x => x.Id == "V-1");
        staleScope.Context.VersionedCategories.Remove(stale);

        // 上下文 B 并发修改令牌并提交
        using (var otherScope = CreateContext(host))
        {
            var current = otherScope.Context.VersionedCategories.Single(x => x.Id == "V-1");
            current.ConcurrencyStamp = Guid.NewGuid().ToString();
            await otherScope.Context.SaveChangesAsync();
        }

        // 上下文 A 提交软删除：WHERE 令牌与库中不一致 → 并发异常
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => staleScope.Context.SaveChangesAsync());
    }

    /// <summary>
    /// stub 删除（仅构造主键、未加载数据库）：UPDATE 仍只含审计列，
    /// 不会把 stub 的其它空值写入数据库，非审计列数据保持完整
    /// </summary>
    [Fact]
    public async Task StubSoftDelete_WithoutLoad_NoDataLoss_OnlyAuditColumns()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            seedScope.Context.Categories.Add(new SoftDeleteCategory("C-1", "apple"));
            await seedScope.Context.SaveChangesAsync();
        }

        using var scope = CreateContext(host);
        // stub 删除：仅设置主键即 Remove，不经过数据库加载
        scope.Context.Categories.Remove(new SoftDeleteCategory("C-1", null));

        host.Interceptor.Reset();
        await scope.Context.SaveChangesAsync();

        var update = Assert.Single(host.Interceptor.UpdateCommands);
        var setColumns = ExtractSetColumns(update);
        Assert.Equal(new HashSet<string> { "is_deleted", "deleter_id", "deleter_name", "deletion_time" }, setColumns);

        using var verifyScope = CreateContext(host);
        var persisted = verifyScope.Context.Categories.IgnoreQueryFilters().Single(x => x.Id == "C-1");
        Assert.True(persisted.IsDeleted);
        Assert.Equal("apple", persisted.Name);
    }

    /// <summary>
    /// stub + 乐观锁删除：仅构造主键（未加载数据库）的 stub 实体实现 <see cref="IOptimisticLock"/>，
    /// 提交应抛 <see cref="DbUpdateConcurrencyException"/> 且行未被删除。
    /// stub 的乐观锁令牌由构造器生成、与库中令牌不一致，WHERE 按该令牌匹配不到行；
    /// 该行为固化后防止未来实现漂移（如重新引入 Reload 导致 stub 删除静默成功）。
    /// </summary>
    [Fact]
    public async Task StubSoftDelete_WithOptimisticLock_ThrowsDbUpdateConcurrencyException()
    {
        using var host = CreateHost();

        using (var seedScope = CreateContext(host))
        {
            seedScope.Context.VersionedCategories.Add(new VersionedCategory("V-1", "apple"));
            await seedScope.Context.SaveChangesAsync();
        }

        using var scope = CreateContext(host);
        // stub 删除：仅设置主键即 Remove，不经过数据库加载；构造器生成的新令牌与库中令牌不一致
        scope.Context.VersionedCategories.Remove(new VersionedCategory("V-1", null));

        // WHERE 按 stub 的乐观锁令牌匹配不到行 → 并发异常
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => scope.Context.SaveChangesAsync());

        // 异常后行必须仍然存在且未被软删除，业务数据保持完整
        using var verifyScope = CreateContext(host);
        var persisted = verifyScope.Context.VersionedCategories.IgnoreQueryFilters().Single(x => x.Id == "V-1");
        Assert.False(persisted.IsDeleted);
        Assert.Equal("apple", persisted.Name);
    }

    /// <summary>
    /// 统计指定子串在 SQL 文本中出现的次数
    /// </summary>
    /// <param name="sql">SQL 文本</param>
    /// <param name="token">要统计的子串</param>
    /// <returns>出现次数</returns>
    private static int CountOccurrences(string sql, string token)
    {
        var count = 0;
        var index = 0;
        while ((index = sql.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
    }
}
