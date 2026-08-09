using System;
using System.Net.Http;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.AspNetCore.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.AspNetCore.Test;

/// <summary>
/// 审计过滤器 IServiceScope 释放回归测试。
/// 通过 scoped 假 IAuditingStore 的 Dispose 计数观测审计 scope 的释放：
/// 假 store 由审计 scope 解析，scope 被释放时其内已解析的 store 实例同步释放。
/// 覆盖路径：正常完成、ISession 缺失跳过、IAuditingStore 缺失、Action 抛异常（N5）。
/// </summary>
public class AuditScopeLeakTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    /// <summary>为当前用例构建独立测试服务器，保证假服务开关与 Dispose 计数互不串扰</summary>
    public AuditScopeLeakTests()
    {
        FakeAuditingStore.DisposeCount = 0;
        AuditLeakTestSettings.RegisterSession = true;
        AuditLeakTestSettings.RegisterStore = true;

        _server = new TestServer(new WebHostBuilder().UseStartup<AuditLeakStartup>());
        _client = _server.CreateClient();
    }

    /// <summary>释放测试服务器资源</summary>
    public void Dispose()
    {
        _server.Dispose();
        _client.Dispose();
    }

    /// <summary>完整审计路径：写请求正常完成后，审计 scope（含已解析的假 store）必须被释放</summary>
    [Fact]
    public async Task NormalPath_ShouldDisposeAuditScope()
    {
        var response = await _client.PostAsync("/audit-leak/ok", new StringContent(""));

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(1, FakeAuditingStore.DisposeCount);
    }

    /// <summary>ISession 缺失跳过路径：写请求跳过审计但 scope 已创建，必须仍被释放（N1 回归）</summary>
    [Fact]
    public async Task SkipPath_NoSession_ShouldDisposeAuditScope()
    {
        AuditLeakTestSettings.RegisterSession = false;
        var server = new TestServer(new WebHostBuilder().UseStartup<AuditLeakStartup>());
        var client = server.CreateClient();

        try
        {
            var response = await client.PostAsync("/audit-leak/ok", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(1, FakeAuditingStore.DisposeCount);
        }
        finally
        {
            server.Dispose();
            client.Dispose();
        }
    }

    /// <summary>IAuditingStore 缺失路径：写请求必须正常完成，不抛异常（scope 未解析服务，靠代码路径兜底）</summary>
    [Fact]
    public async Task StoreMissingPath_ShouldCompleteNormally()
    {
        AuditLeakTestSettings.RegisterStore = false;
        var server = new TestServer(new WebHostBuilder().UseStartup<AuditLeakStartup>());
        var client = server.CreateClient();

        try
        {
            var response = await client.PostAsync("/audit-leak/ok", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
        }
        finally
        {
            server.Dispose();
            client.Dispose();
        }
    }

    /// <summary>
    /// Action 抛异常路径（N5）：全局异常过滤器处理后，审计 scope 必须仍被释放且仅释放一次。
    /// ASP.NET Core 10 中 action 异常不传播进 OnActionExecutionAsync 的 catch，
    /// 仅 OnActionExecuted 回调携带 context.Exception，故该用例曾标记 Skip，
    /// 修复 OnActionExecuted 释放 scope 后启用。
    /// </summary>
    [Fact]
    public async Task ExceptionPath_ShouldDisposeAuditScope()
    {
        var response = await _client.PostAsync("/audit-leak/throw", new StringContent(""));

        Assert.Equal(500, (int)response.StatusCode);
        Assert.Equal(1, FakeAuditingStore.DisposeCount);
    }
}

/// <summary>审计 scope 泄漏测试专用 Startup 的依赖注册开关（同测试 class 内用例串行执行，互不串扰）</summary>
internal static class AuditLeakTestSettings
{
    /// <summary>是否注册 ISession；false 时审计走「会话缺失跳过」路径</summary>
    public static bool RegisterSession = true;

    /// <summary>是否注册 IAuditingStore；false 时审计走「存储缺失」路径</summary>
    public static bool RegisterStore = true;
}

/// <summary>审计 scope 泄漏测试专用控制器：提供正常完成与抛异常两条写请求路径</summary>
[ApiController]
[Route("audit-leak")]
public class AuditLeakController : ControllerBase
{
    /// <summary>正常完成的写请求</summary>
    /// <returns>空响应</returns>
    [HttpPost("ok")]
    public IActionResult Success()
    {
        return Ok();
    }

    /// <summary>抛出异常的写请求，触发全局异常过滤器处理（N5 泄漏路径）</summary>
    /// <returns>恒抛异常，无返回值</returns>
    [HttpPost("throw")]
    public IActionResult Throw()
    {
        throw new InvalidOperationException("boom");
    }
}

/// <summary>审计 scope 泄漏测试专用 Startup：仅注册最小依赖（假 store/假会话/假工作单元）</summary>
public class AuditLeakStartup
{
    /// <summary>注册控制器、框架过滤器与测试假服务</summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(x =>
        {
            x.Filters.AddUnitOfWork().AddAudit().AddGlobalException().AddResponseWrapper();
        });

        // 未注册 IUnitOfWork 时审计过滤器直接跳过，故必须提供假实现
        services.AddScoped<IUnitOfWork>(_ => new FakeUnitOfWork());
        // 未注册 ISession 时审计走「会话缺失跳过」路径，通过开关模拟
        if (AuditLeakTestSettings.RegisterSession)
        {
            services.AddScoped<ISession>(_ => new FakeSession());
        }

        // 未注册 IAuditingStore 时审计走「存储缺失」路径，通过开关模拟
        if (AuditLeakTestSettings.RegisterStore)
        {
            services.AddScoped<IAuditingStore>(_ => new FakeAuditingStore());
        }
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

/// <summary>
/// 审计测试用假工作单元（需为 public 以便测试专用 Startup 实例化）。
/// 保证审计过滤器进入审计流程（未注册 IUnitOfWork 时审计直接跳过）。
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    /// <summary>保存完成事件，测试场景不使用</summary>
    public event Action SavedChanges;

    /// <summary>空实现：不执行持久化</summary>
    /// <param name="auditOperation">审计操作，测试场景不校验</param>
    public void RegisterAuditOperation(AuditOperation auditOperation)
    {
    }

    /// <summary>空实现：测试场景无需真实保存</summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已完成的任务</returns>
    public Task SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>空实现：无托管资源</summary>
    public void Dispose()
    {
    }
}

/// <summary>审计测试用假会话：返回固定用户标识，使审计可完整构建 AuditOperation</summary>
public sealed class FakeSession : ISession
{
    /// <summary>跟踪标识</summary>
    public string TraceIdentifier => "trace-1";

    /// <summary>用户标识</summary>
    public string UserId => "u1";

    /// <summary>用户名</summary>
    public string UserName => "user1";

    /// <summary>用户邮箱</summary>
    public string Email => "u1@example.com";

    /// <summary>用户电话</summary>
    public string PhoneNumber => "13800000000";

    /// <summary>用户显示名称</summary>
    public string UserDisplayName => "用户一";

    /// <summary>用户角色集合</summary>
    public System.Collections.Generic.IReadOnlyCollection<string> Roles => [];

    /// <summary>用户主体集合</summary>
    public System.Collections.Generic.IReadOnlyCollection<string> Subjects => [];

    /// <summary>按字段返回 null，使设备信息各字段保持未设置</summary>
    /// <param name="field">会话字段</param>
    /// <returns>null</returns>
    public string GetValue(SessionField field)
    {
        return null;
    }

    /// <summary>空实现：测试场景不加载会话</summary>
    /// <param name="session">源会话</param>
    public void Load(ISession session)
    {
    }
}

/// <summary>
/// 审计测试用假审计存储（需为 public 以便测试专用 Startup 实例化）。
/// 记录 scope 释放次数：实例由审计 scope 解析，scope 释放时同步释放。
/// </summary>
public sealed class FakeAuditingStore : IAuditingStore, IDisposable
{
    /// <summary>已释放次数（静态累计，每用例开始前归零）</summary>
    public static int DisposeCount;

    /// <summary>空实现：本测试仅观测 scope 释放，不校验审计落库</summary>
    /// <param name="auditOperation">审计操作，不参与断言</param>
    /// <returns>已完成的任务</returns>
    public Task AddAsync(AuditOperation auditOperation)
    {
        return Task.CompletedTask;
    }

    /// <summary>scope 释放回调，累计释放计数</summary>
    public void Dispose()
    {
        DisposeCount++;
    }
}
