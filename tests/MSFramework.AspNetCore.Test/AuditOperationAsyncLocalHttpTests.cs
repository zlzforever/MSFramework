using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.AspNetCore.Test;

/// <summary>
/// 审计操作 AsyncLocal 承载的 HTTP 全链路回归测试：
/// 验证 Audit 过滤器将审计操作写入 <see cref="AuditOperationContext"/>（AsyncLocal）后，
/// 真实 <see cref="EfUnitOfWork"/> 保存回调能收集变更实体，结果阶段保存到审计存储；
/// 以及顺序请求之间审计操作不串扰（清理语义生效）。
/// </summary>
public class AuditOperationAsyncLocalHttpTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly SqliteConnection _connection;

    /// <summary>为当前用例构建独立测试服务器与内存数据库，保证静态捕获集合互不串扰</summary>
    public AuditOperationAsyncLocalHttpTests()
    {
        AuditFlowHttpSettings.ObservedOperationIds.Clear();
        AuditFlowHttpSettings.NextOrderId = 1;
        CapturingAuditingStore.Reset();

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        AuditFlowHttpSettings.Connection = _connection;

        _server = new TestServer(new WebHostBuilder().UseStartup<AuditFlowHttpStartup>());
        using (var scope = _server.Host.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<AuditFlowHttpContext>().Database.EnsureCreated();
        }

        _client = _server.CreateClient();
    }

    /// <summary>释放测试服务器、客户端与内存数据库连接</summary>
    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
        _connection.Dispose();
        AuditFlowHttpSettings.Connection = null;
    }

    /// <summary>
    /// 完整审计链路：写请求经 Audit 过滤器设置 AsyncLocal 审计操作后，
    /// UnitOfWork 过滤器保存时收集变更实体，结果阶段将含实体的审计操作写入审计存储
    /// </summary>
    [Fact]
    public async Task FullChain_CollectsEntitiesAndSavesToStore()
    {
        var response = await _client.PostAsync("/audit-flow/create", new StringContent(""));

        Assert.Equal(200, (int)response.StatusCode);

        var captured = Assert.Single(CapturingAuditingStore.Captured);
        var entity = Assert.Single(captured.Entities);
        Assert.Equal("ORD-HTTP-1", entity.EntityId);
        Assert.Equal(OperationType.Add, entity.OperationType);
        Assert.Same(captured, entity.Operation);

        // Action 中观察到的执行流审计操作必须与最终落库的是同一个实例
        var observedId = Assert.Single(AuditFlowHttpSettings.ObservedOperationIds);
        Assert.Equal(captured.Id, observedId);
    }

    /// <summary>
    /// 顺序请求隔离：前一个请求的审计操作随请求结束被清理，
    /// 后一个请求的执行流必须读到自己的新审计操作（不串扰）
    /// </summary>
    [Fact]
    public async Task SequentialRequests_DoNotPolluteEachOther()
    {
        var firstResponse = await _client.PostAsync("/audit-flow/create", new StringContent(""));
        var secondResponse = await _client.PostAsync("/audit-flow/create", new StringContent(""));

        Assert.Equal(200, (int)firstResponse.StatusCode);
        Assert.Equal(200, (int)secondResponse.StatusCode);

        Assert.Equal(2, CapturingAuditingStore.Captured.Count);
        Assert.Equal(2, AuditFlowHttpSettings.ObservedOperationIds.Count);

        var firstCaptured = CapturingAuditingStore.Captured[0];
        var secondCaptured = CapturingAuditingStore.Captured[1];

        // 两个请求各自持有独立的审计操作，互不串扰
        Assert.NotSame(firstCaptured, secondCaptured);
        Assert.Equal(AuditFlowHttpSettings.ObservedOperationIds[0], firstCaptured.Id);
        Assert.Equal(AuditFlowHttpSettings.ObservedOperationIds[1], secondCaptured.Id);

        // 各请求的变更实体只收集到各自请求的审计操作上
        Assert.Equal("ORD-HTTP-1", Assert.Single(firstCaptured.Entities).EntityId);
        Assert.Equal("ORD-HTTP-2", Assert.Single(secondCaptured.Entities).EntityId);
    }
}

/// <summary>HTTP 全链路审计测试的运行时设置（SQLite 连接与 Action 观察到的执行流审计操作标识）</summary>
internal static class AuditFlowHttpSettings
{
    /// <summary>测试服务器使用的 SQLite 内存连接（由测试类持有并传入 Startup）</summary>
    public static SqliteConnection Connection { get; set; }

    /// <summary>各请求 Action 执行时观察到的 AuditOperationContext.Value.Id（按请求顺序记录）</summary>
    public static List<string> ObservedOperationIds { get; } = [];

    /// <summary>下一条订单标识序号（按请求顺序自增，保证主键唯一）</summary>
    public static int NextOrderId { get; set; } = 1;
}

    /// <summary>HTTP 全链路审计测试专用控制器：写请求向 DbContext 添加实体并记录执行流审计操作</summary>
    [ApiController]
    [Route("audit-flow")]
    public class AuditFlowHttpController : ControllerBase
    {
        /// <summary>创建订单：触发审计过滤器设置 AsyncLocal，随后由 UnitOfWork 过滤器保存</summary>
        /// <returns>空响应</returns>
        [HttpPost("create")]
        public IActionResult Create([FromServices] AuditFlowHttpContext context)
        {
            var orderId = $"ORD-HTTP-{AuditFlowHttpSettings.NextOrderId++}";
            context.Orders.Add(new AuditFlowOrder(orderId, "order-" + orderId));
            AuditFlowHttpSettings.ObservedOperationIds.Add(AuditOperationContext.Value?.Id);
            return Ok();
        }
    }

/// <summary>HTTP 全链路审计测试专用实体（string 主键聚合根）</summary>
public class AuditFlowOrder : DeletionAggregateRoot<string>
{
    protected AuditFlowOrder() : base(default!)
    {
    }

    /// <summary>
    /// 创建测试订单实体
    /// </summary>
    /// <param name="id">订单标识</param>
    /// <param name="name">订单名称</param>
    public AuditFlowOrder(string id, string name) : base(id)
    {
        Name = name;
    }

    /// <summary>
    /// 订单名称
    /// </summary>
    public string Name { get; set; }
}

/// <summary>HTTP 全链路审计测试专用 DbContext</summary>
public class AuditFlowHttpContext(DbContextOptions<AuditFlowHttpContext> options) : DbContextBase(options)
{
    /// <summary>
    /// 订单集合
    /// </summary>
    public DbSet<AuditFlowOrder> Orders => Set<AuditFlowOrder>();

    /// <inheritdoc />
    protected override void ApplyConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditFlowOrder>(builder =>
            builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
    }
}

/// <summary>HTTP 全链路审计测试专用实体配置查找器：将实体映射到测试 DbContext</summary>
public class AuditFlowHttpEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
{
    private static readonly Dictionary<Type, Type> EntityToDbContext = new()
    {
        [typeof(AuditFlowOrder)] = typeof(AuditFlowHttpContext)
    };

    /// <inheritdoc />
    public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

    /// <inheritdoc />
    public Type GetDbContextTypeForEntity(Type entityType) =>
        EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

    /// <inheritdoc />
    public IEnumerable<Type> GetAllDbContextTypes() => EntityToDbContext.Values.Distinct();

    /// <inheritdoc />
    public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
}

/// <summary>HTTP 全链路审计测试专用作用域服务提供程序：包装预构建的根容器</summary>
public class AuditFlowHttpScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
{
    /// <inheritdoc />
    public T GetService<T>() => serviceProvider.GetService<T>();
}

/// <summary>HTTP 全链路审计测试专用 Startup：注册框架过滤器、EF 服务与测试假服务</summary>
public class AuditFlowHttpStartup
{
    /// <summary>注册控制器、框架过滤器、EF 扩展与测试假服务</summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(x =>
        {
            x.Filters.AddUnitOfWork().AddAudit().AddGlobalException().AddResponseWrapper();
        });

        // 初始化框架（程序集扫描），AddEntityFrameworkExtension 依赖 Utils.Runtime 类型缓存
        services.AddMicroserviceFramework();

        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new AuditFlowHttpEntityConfigurationTypeFinder());
        services.AddDbContext<AuditFlowHttpContext>(options => options.UseSqlite(AuditFlowHttpSettings.Connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new AuditFlowHttpScopeServiceProvider(rootProvider));

        // 注册 IUnitOfWork→EfUnitOfWork、DbContextFactory 等 EF 扩展服务
        services.AddEntityFrameworkExtension();

        services.AddScoped<ISession>(_ => new FakeSession());
        services.AddScoped<IAuditingStore>(_ => new CapturingAuditingStore());
    }

    /// <summary>配置请求管道：路由到控制器</summary>
    /// <param name="app">应用构建器</param>
    /// <param name="env">宿主环境</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(builder => { builder.MapControllers(); });
    }
}

/// <summary>捕获审计操作的测试审计存储：记录每次保存的审计操作供断言</summary>
public sealed class CapturingAuditingStore : IAuditingStore
{
    /// <summary>已捕获的审计操作集合（每用例开始前清空）</summary>
    public static List<AuditOperation> Captured { get; } = [];

    /// <summary>清空捕获集合</summary>
    public static void Reset()
    {
        Captured.Clear();
    }

    /// <summary>记录审计操作并返回已完成任务</summary>
    /// <param name="auditOperation">审计操作</param>
    /// <returns>已完成的任务</returns>
    public Task AddAsync(AuditOperation auditOperation)
    {
        Captured.Add(auditOperation);
        return Task.CompletedTask;
    }
}
