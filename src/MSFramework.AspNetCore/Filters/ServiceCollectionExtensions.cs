using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="filters"></param>
    extension(FilterCollection filters)
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public FilterCollection AddUnitOfWork()
        {
            filters.Add<UnitOfWork>(Constants.UnitOfWork);
            return filters;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public FilterCollection AddAudit()
        {
            filters.Add<Audit>(Constants.Audit);
            return filters;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public FilterCollection AddResponseWrapper()
        {
            filters.Add<ResponseWrapperFilter>(Constants.ResponseWrapper);
            return filters;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public FilterCollection AddGlobalException()
        {
            filters.Add<GlobalExceptionFilter>(Constants.GlobalException);
            return filters;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseDaprSecurity(this WebApplication app)
    {
        app.UseMiddleware<DaprSecurityMiddleware>();
        return app;
    }
}
