using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///     注册框架内置 Filter 的扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="filters"></param>
    extension(FilterCollection filters)
    {
        /// <summary>
        ///     添加工作单元过滤器，在请求结束后自动提交事务
        /// </summary>
        /// <returns>过滤器集合</returns>
        public FilterCollection AddUnitOfWork()
        {
            filters.Add<UnitOfWork>(Constants.UnitOfWork);
            return filters;
        }

        /// <summary>
        ///     添加审计过滤器，记录操作日志
        /// </summary>
        /// <returns>过滤器集合</returns>
        public FilterCollection AddAudit()
        {
            filters.Add<AuditAttribute>(Constants.Audit);
            return filters;
        }

        /// <summary>
        ///     添加响应包装过滤器，将返回值统一包装为 ApiResult
        /// </summary>
        /// <returns>过滤器集合</returns>
        public FilterCollection AddResponseWrapper()
        {
            filters.Add<ResponseWrapperFilter>(Constants.ResponseWrapper);
            return filters;
        }

        /// <summary>
        ///     添加全局异常过滤器，捕获未处理异常并返回统一错误响应
        /// </summary>
        /// <returns>过滤器集合</returns>
        public FilterCollection AddGlobalException()
        {
            filters.Add<GlobalExceptionFilter>(Constants.GlobalException);
            return filters;
        }
    }

    /// <summary>
    ///     <b>已废弃</b>：启用 Dapr Topic 网络层安全中间件（仅放行 loopback 来源的 Topic 回调）。
    ///     鉴权请改用 Dapr 官方 App API Token 方案：
    ///     <c>AddAuthentication().AddDapr()</c> + <c>AddAuthorization(o => o.AddDapr())</c>
    ///     + Topic 端点 <c>[Authorize(Policy = "Dapr")]</c>；
    ///     同时在网关上禁止访问 <c>/dapr/subscribe</c>。
    ///     存量宿主在迁移到官方方案前，可继续调用本方法保留网络层防御
    /// </summary>
    /// <param name="app">Web 应用</param>
    /// <returns>Web 应用</returns>
    [Obsolete("启用 Dapr Topic 网络层安全中间件已废弃，请改用 Dapr 官方 App API Token 方案。")]
    public static WebApplication UseDaprSecurity(this WebApplication app)
    {
        // 存量宿主迁移到官方 App API Token 方案前，保留网络层纵深防御；
        // 类已标注 [Obsolete]，此处有意压制编译告警
#pragma warning disable CS0618
        app.UseMiddleware<DaprSecurityMiddleware>();
#pragma warning restore CS0618
        return app;
    }
}
