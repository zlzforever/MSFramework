using System;
using System.Net.Http;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.AspNetCore.Test;

/// <summary>
/// 审计过滤器异常路径回归测试（N6/N7）：
/// 用模拟 scoped 生命周期探针存储（scope 释放后 AddAsync 抛异常，等价于默认
/// EfAuditingStore 持 DbContext 被 Dispose 后的行为）观测：
/// <list type="bullet">
/// <item><description>N5/N6：异常被异常过滤器处理（全局/用户自定义）时，保存发生在 scope 释放之前且真实落库、仅保存一次；</description></item>
/// <item><description>N7：异常直接传播（无异常过滤器处理）时不保存审计；</description></item>
/// <item><description>两条路径均无 scope 泄漏、无重复释放。</description></item>
/// </list>
/// </summary>
public class AuditExceptionPathTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    /// <summary>为当前用例构建独立测试服务器，保证探针计数与过滤器注册开关互不串扰</summary>
    public AuditExceptionPathTests()
    {
        LifecycleProbeAuditingStore.Reset();
        AuditExceptionPathSettings.RegisterGlobalExceptionFilter = true;
        AuditExceptionPathSettings.RegisterUserExceptionFilter = false;

        _server = new TestServer(new WebHostBuilder().UseStartup<AuditExceptionPathStartup>());
        _client = _server.CreateClient();
    }

    /// <summary>释放测试服务器资源</summary>
    public void Dispose()
    {
        _server.Dispose();
        _client.Dispose();
    }

    /// <summary>
    /// N5/N6 回归：Action 抛异常且被全局异常过滤器处理后，
    /// 审计必须在 scope 释放之前保存（真实落库，探针在 Dispose 后 AddAsync 会抛异常），
    /// 且仅保存一次、scope 仅释放一次
    /// </summary>
    [Fact]
    public async Task HandledException_GlobalExceptionFilter_SavesAuditBeforeScopeDispose()
    {
        var response = await _client.PostAsync("/audit-exc/throw", new StringContent(""));

        Assert.Equal(500, (int)response.StatusCode);

        // 保存发生在 scope 释放之前：若保存时序错误（先释放后保存），
        // 探针 AddAsync 会命中已释放状态而抛异常，AddCallsBeforeDispose 为 0
        Assert.Equal(1, LifecycleProbeAuditingStore.AddCallsBeforeDispose);
        // 无重复保存
        Assert.Equal(1, LifecycleProbeAuditingStore.AddCallCount);
        // scope 无泄漏、无重复释放
        Assert.Equal(1, LifecycleProbeAuditingStore.DisposeCount);
    }

    /// <summary>
    /// N5/N6 扩展：Action 抛异常且被用户自定义异常过滤器处理时，
    /// 审计同样必须保存且先于 scope 释放（异常兜底过滤器 Order 取最小值，
    /// 在用户异常过滤器之后执行，ExceptionHandled 为 true 时不再干预，交给结果阶段保存）
    /// </summary>
    [Fact]
    public async Task HandledException_UserExceptionFilter_SavesAuditBeforeScopeDispose()
    {
        AuditExceptionPathSettings.RegisterGlobalExceptionFilter = false;
        AuditExceptionPathSettings.RegisterUserExceptionFilter = true;
        var server = new TestServer(new WebHostBuilder().UseStartup<AuditExceptionPathStartup>());
        var client = server.CreateClient();

        try
        {
            var response = await client.PostAsync("/audit-exc/throw", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(1, LifecycleProbeAuditingStore.AddCallsBeforeDispose);
            Assert.Equal(1, LifecycleProbeAuditingStore.AddCallCount);
            Assert.Equal(1, LifecycleProbeAuditingStore.DisposeCount);
        }
        finally
        {
            server.Dispose();
            client.Dispose();
        }
    }

    /// <summary>
    /// N7 回归：未注册任何异常过滤器时 Action 异常直接传播（MVC 在过滤器链之外重抛，
    /// 结果阶段被跳过），审计必须不保存，且 scope 仍被释放、仅释放一次（无泄漏）
    /// </summary>
    [Fact]
    public async Task UnhandledException_DoesNotSaveAuditAndDisposesScope()
    {
        AuditExceptionPathSettings.RegisterGlobalExceptionFilter = false;
        AuditExceptionPathSettings.RegisterUserExceptionFilter = false;
        var server = new TestServer(new WebHostBuilder().UseStartup<AuditExceptionPathStartup>());
        var client = server.CreateClient();

        try
        {
            // 异常直接传播：TestServer 侧请求处理失败（客户端表现为异常或 500），
            // 是否抛出不影响本用例断言——关键断言是审计未保存且 scope 已释放
            try
            {
                await client.PostAsync("/audit-exc/throw", new StringContent(""));
            }
            catch (Exception)
            {
                // 异常直接传播的预期表现
            }

            // 异常直接传播路径不保存审计（N7 契约）
            Assert.Equal(0, LifecycleProbeAuditingStore.AddCallCount);
            // scope 无泄漏：异常阶段兜底释放，且仅释放一次
            Assert.Equal(1, LifecycleProbeAuditingStore.DisposeCount);
        }
        finally
        {
            server.Dispose();
            client.Dispose();
        }
    }

    /// <summary>正常完成路径回归：审计保存先于 scope 释放、仅保存一次、scope 仅释放一次</summary>
    [Fact]
    public async Task NormalPath_SavesAuditBeforeScopeDispose()
    {
        var response = await _client.PostAsync("/audit-exc/ok", new StringContent(""));

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(1, LifecycleProbeAuditingStore.AddCallsBeforeDispose);
        Assert.Equal(1, LifecycleProbeAuditingStore.AddCallCount);
        Assert.Equal(1, LifecycleProbeAuditingStore.DisposeCount);
    }
}

/// <summary>异常路径审计测试专用 Startup 的依赖注册开关（同测试 class 内用例串行执行，互不串扰）</summary>
internal static class AuditExceptionPathSettings
{
    /// <summary>是否注册全局异常过滤器；false 时走「异常直接传播」路径</summary>
    public static bool RegisterGlobalExceptionFilter = true;

    /// <summary>是否注册用户自定义异常过滤器；true 时模拟第三方异常过滤器处理异常的场景</summary>
    public static bool RegisterUserExceptionFilter;
}

/// <summary>异常路径审计测试专用控制器：提供正常完成与抛异常两条写请求路径</summary>
[ApiController]
[Route("audit-exc")]
public class AuditExceptionController : ControllerBase
{
    /// <summary>正常完成的写请求</summary>
    /// <returns>空响应</returns>
    [HttpPost("ok")]
    public IActionResult Success()
    {
        return Ok();
    }

    /// <summary>抛出异常的写请求，触发异常过滤器处理或异常直接传播</summary>
    /// <returns>恒抛异常，无返回值</returns>
    [HttpPost("throw")]
    public IActionResult Throw()
    {
        throw new InvalidOperationException("boom");
    }
}

/// <summary>异常路径审计测试专用 Startup：注册控制器、框架过滤器与生命周期探针存储</summary>
public class AuditExceptionPathStartup
{
    /// <summary>注册控制器、框架过滤器与测试假服务</summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(x =>
        {
            x.Filters.AddUnitOfWork().AddAudit().AddResponseWrapper();
            if (AuditExceptionPathSettings.RegisterGlobalExceptionFilter)
            {
                x.Filters.AddGlobalException();
            }

            if (AuditExceptionPathSettings.RegisterUserExceptionFilter)
            {
                x.Filters.Add<HandlingExceptionFilter>();
            }
        });

        services.AddScoped<IUnitOfWork>(_ => new FakeUnitOfWork());
        services.AddScoped<ISession>(_ => new FakeSession());
        services.AddScoped<IAuditingStore>(_ => new LifecycleProbeAuditingStore());
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
/// 用户自定义异常过滤器（模拟第三方异常过滤器）：处理 <see cref="InvalidOperationException"/>
/// 并设置统一响应，用于验证「用户自定义异常过滤器处理异常时审计仍保存」的场景
/// </summary>
public class HandlingExceptionFilter : IExceptionFilter
{
    /// <summary>处理 InvalidOperationException，标记已处理并返回 200 响应</summary>
    /// <param name="context">异常上下文</param>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidOperationException)
        {
            context.Result = new ObjectResult("handled-by-user-filter");
            context.ExceptionHandled = true;
        }
    }
}

/// <summary>
/// 模拟 scoped 生命周期探针审计存储：scope 释放（Dispose）后 AddAsync 抛异常，
/// 等价于默认 EfAuditingStore 持 DbContext 被 Dispose 后的行为。
/// 静态计数用于断言：保存是否发生在 scope 释放之前、保存次数、释放次数。
/// </summary>
public sealed class LifecycleProbeAuditingStore : IAuditingStore, IDisposable
{
    /// <summary>AddAsync 总调用次数（含命中已释放状态抛异常的场景）</summary>
    public static int AddCallCount;

    /// <summary>scope 存活期间 AddAsync 调用次数（即真实落库次数）</summary>
    public static int AddCallsBeforeDispose;

    /// <summary>scope 释放次数</summary>
    public static int DisposeCount;

    /// <summary>当前是否已释放（scope 释放后置 true）</summary>
    private static bool _disposed;

    /// <summary>重置探针静态计数（每用例开始前调用）</summary>
    public static void Reset()
    {
        AddCallCount = 0;
        AddCallsBeforeDispose = 0;
        DisposeCount = 0;
        _disposed = false;
    }

    /// <summary>
    /// 记录保存调用；scope 已释放时抛 <see cref="ObjectDisposedException"/>，
    /// 模拟默认 EfAuditingStore 持 DbContext 被 Dispose 后 AddAsync 的失败行为
    /// </summary>
    /// <param name="auditOperation">审计操作，本探针仅计数不做持久化</param>
    /// <returns>已完成的任务</returns>
    public Task AddAsync(AuditOperation auditOperation)
    {
        AddCallCount++;
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LifecycleProbeAuditingStore), "审计 scope 已释放");
        }

        AddCallsBeforeDispose++;
        return Task.CompletedTask;
    }

    /// <summary>scope 释放回调：标记已释放并累计释放计数</summary>
    public void Dispose()
    {
        _disposed = true;
        DisposeCount++;
    }
}
