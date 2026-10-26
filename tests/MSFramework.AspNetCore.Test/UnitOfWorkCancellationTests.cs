using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MSFramework.AspNetCore.Test;

/// <summary>验证 HTTP 写请求的取消信号传递至工作单元，不连接数据库。</summary>
public class UnitOfWorkCancellationTests
{
    /// <summary>成功写操作提交时必须传递 HttpContext.RequestAborted。</summary>
    [Fact]
    public async Task CommitReceivesRequestAbortedToken()
    {
        using var cancellation = new CancellationTokenSource();
        var work = new RecordingUnitOfWork();
        using var provider = new ServiceCollection().AddSingleton<IUnitOfWork>(work).BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = provider, RequestAborted = cancellation.Token };
        httpContext.Request.Method = HttpMethods.Post;
        var action = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
        });
        var filters = new List<IFilterMetadata>();
        var controller = new object();
        var executing = new ActionExecutingContext(action, filters, new Dictionary<string, object>(), controller);
        var executed = new ActionExecutedContext(action, filters, controller);
        var filter = new UnitOfWork(NullLogger<UnitOfWork>.Instance);
        await filter.OnActionExecutionAsync(executing, () => Task.FromResult(executed));
        Assert.Equal(cancellation.Token, work.ReceivedToken);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        /// <summary>最近一次保存收到的取消令牌。</summary>
        public CancellationToken ReceivedToken { get; private set; }
        /// <inheritdoc/>
        public event Func<CancellationToken, ValueTask> SavedChanges { add { } remove { } }
        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
        /// <inheritdoc/>
        public void Dispose() { }
    }
}
