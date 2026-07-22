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
            filters.Add<Audit>(Constants.Audit);
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
    ///     启用 Dapr API 令牌安全中间件，验证请求中的 Dapr API 令牌
    /// </summary>
    /// <param name="app">Web 应用</param>
    /// <returns>Web 应用</returns>
    public static WebApplication UseDaprSecurity(this WebApplication app)
    {
        app.UseMiddleware<DaprSecurityMiddleware>();
        return app;
    }
}
