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
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 工作单元回调异常分流回归测试：
/// 验证主上下文提交成功后，<see cref="IUnitOfWork.SavedChanges"/> 回调（集成事件发布等）
/// 抛异常时——不冒泡改变提交结果（数据仍落库），且异常回调不影响后续回调继续执行；
/// 以及无可写上下文（无实体配置）时不触发回调的语义。
/// </summary>
public class UnitOfWorkCallbackTests
{
    /// <summary>
    /// 测试用实体（字符串主键创建审计聚合根）
    /// </summary>
    public class CallbackEntity : CreationAggregateRoot<string>
    {
        /// <summary>仅供 EF Core 物化使用</summary>
        protected CallbackEntity() : base(default!)
        {
        }

        /// <summary>初始化实体</summary>
        public CallbackEntity(string id) : base(id)
        {
        }
    }

    /// <summary>
    /// 测试用 DbContext
    /// </summary>
    public class UnitOfWorkCallbackContext(DbContextOptions<UnitOfWorkCallbackContext> options)
        : DbContextBase(options)
    {
        /// <summary>测试实体集合</summary>
        public DbSet<CallbackEntity> Entities => Set<CallbackEntity>();

        /// <summary>配置主键列长度</summary>
        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallbackEntity>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器：把测试实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(CallbackEntity)] = typeof(UnitOfWorkCallbackContext)
        };

        /// <summary>获取指定上下文的实体配置集合（测试场景无需映射配置）</summary>
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        /// <summary>返回实体所属的 DbContext 类型，未注册返回 null</summary>
        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        /// <summary>全部已注册的 DbContext 类型（去重）</summary>
        public IEnumerable<Type> GetAllDbContextTypes() => EntityToDbContext.Values.Distinct();

        /// <summary>判断实体是否已注册上下文映射</summary>
        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接与服务容器，保证连接在测试生命周期内存活
    /// </summary>
    private sealed class TestHost : IDisposable
    {
        /// <summary>构造测试宿主</summary>
        public TestHost(ServiceProvider provider, DbConnection connection)
        {
            Provider = provider;
            Connection = connection;
        }

        /// <summary>根服务提供者</summary>
        public ServiceProvider Provider { get; }

        /// <summary>共享的 SQLite 内存连接</summary>
        public DbConnection Connection { get; }

        /// <summary>释放连接</summary>
        public void Dispose() => Connection.Dispose();
    }

    /// <summary>
    /// 测试用会话：提供固定用户审计信息
    /// </summary>
    private sealed class TestSession : ISession
    {
        /// <summary>当前请求的跟踪标识</summary>
        public string TraceIdentifier => "uow-callback-test";

        /// <summary>用户标识</summary>
        public string UserId => "u1";

        /// <summary>用户名</summary>
        public string UserName => "user-1";

        /// <summary>用户邮箱</summary>
        public string Email => null;

        /// <summary>用户电话</summary>
        public string PhoneNumber => null;

        /// <summary>用户的显示名称</summary>
        public string UserDisplayName => "user-1";

        /// <summary>用户角色集合（空）</summary>
        public IReadOnlyCollection<string> Roles => [];

        /// <summary>用户主体集合（空）</summary>
        public IReadOnlyCollection<string> Subjects => [];

        /// <summary>提取请求头字段值（测试场景恒为 null）</summary>
        public string GetValue(SessionField field) => null;

        /// <summary>覆盖当前用户信息（测试场景空实现）</summary>
        public void Load(ISession session)
        {
        }
    }

    /// <summary>
    /// 测试用作用域服务提供程序：把根容器透传给上下文的作用域解析
    /// </summary>
    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        /// <summary>按类型解析服务</summary>
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    /// <summary>
    /// 构建测试服务容器：注册测试实体查找器、DbContext（SQLite 内存库）、
    /// 并通过 <see cref="ServiceCollectionExtensions.AddEntityFrameworkExtension"/>
    /// 注册 <c>IUnitOfWork</c> 等框架服务（TryAdd 不会覆盖已注册的测试查找器）
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddScoped<ISession>(_ => new TestSession());
        services.AddDbContext<UnitOfWorkCallbackContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        services.AddEntityFrameworkExtension();
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<UnitOfWorkCallbackContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 无映射的测试用实体配置查找器：GetAllDbContextTypes 返回空集合，
    /// 使 <see cref="DbContextFactory.GetAllDbContexts"/> 解析不到任何上下文
    /// </summary>
    private sealed class EmptyEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        /// <summary>获取指定上下文的实体配置集合（恒为空）</summary>
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        /// <summary>返回实体所属的 DbContext 类型（恒为 null：无映射）</summary>
        public Type GetDbContextTypeForEntity(Type entityType) => null;

        /// <summary>全部已注册的 DbContext 类型（恒为空）</summary>
        public IEnumerable<Type> GetAllDbContextTypes() => [];

        /// <summary>判断实体是否已注册上下文映射（恒为 false）</summary>
        public bool HasDbContextForEntity<T>() => false;
    }

    /// <summary>
    /// 提交成功后回调抛异常：异常被分流不冒泡，后续回调仍执行，数据正常落库
    /// </summary>
    [Fact]
    public async Task CallbackExceptionDoesNotBubbleAndFollowingCallbacksStillRun()
    {
        using var host = CreateHost();
        using var scope = host.Provider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var followingCallbackExecuted = false;
        Func<CancellationToken, ValueTask> throwingCallback = _ =>
            throw new InvalidOperationException("simulated integration event publish failure");
        Func<CancellationToken, ValueTask> followingCallback = _ =>
        {
            followingCallbackExecuted = true;
            return ValueTask.CompletedTask;
        };
        unitOfWork.SavedChanges += throwingCallback;
        unitOfWork.SavedChanges += followingCallback;

        // 提交 + 回调：不应因回调异常而抛出
        var entity = new CallbackEntity(Guid.NewGuid().ToString("N"));
        scope.ServiceProvider.GetRequiredService<UnitOfWorkCallbackContext>().Entities.Add(entity);
        await unitOfWork.SaveChangesAsync();

        // 后续回调未受异常回调影响，数据正常落库
        Assert.True(followingCallbackExecuted);
        using var verifyContext = new UnitOfWorkCallbackContext(
            host.Provider.GetRequiredService<DbContextOptions<UnitOfWorkCallbackContext>>());
        Assert.NotNull(await verifyContext.Entities.FindAsync(entity.Id));
    }

    /// <summary>
    /// 无可写上下文（无实体映射）时不触发 SavedChanges 回调——
    /// "没有保存"不应被通知为"保存完成"
    /// </summary>
    [Fact]
    public async Task NoDbContextFiresNoCallback()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        try
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEntityConfigurationTypeFinder>(new EmptyEntityConfigurationTypeFinder());
            services.AddEntityFrameworkExtension();
            var provider = services.BuildServiceProvider();

            var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
            var callbackFired = false;
            unitOfWork.SavedChanges += _ =>
            {
                callbackFired = true;
                return ValueTask.CompletedTask;
            };

            await unitOfWork.SaveChangesAsync();

            Assert.False(callbackFired);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
}
