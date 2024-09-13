using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static FilterCollection AddUnitOfWork(this FilterCollection filters)
    {
        filters.Add<UnitOfWork>(Constants.UnitOfWork);
        return filters;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static FilterCollection AddAudit(this FilterCollection filters)
    {
        filters.Add<Audit>(Constants.Audit);
        return filters;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static FilterCollection AddResponseWrapper(this FilterCollection filters)
    {
        filters.Add<ResponseWrapperFilter>(Constants.ResponseWrapper);
        return filters;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static FilterCollection AddGlobalException(this FilterCollection filters)
    {
        filters.Add<GlobalExceptionFilter>(Constants.GlobalException);
        return filters;
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
