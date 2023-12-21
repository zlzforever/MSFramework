using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

public static class ServiceCollectionExtensions
{
    public static FilterCollection AddUnitOfWork(this FilterCollection filters)
    {
        filters.Add<UnitOfWork>(Constants.UnitOfWork);
        return filters;
    }

    public static FilterCollection AddAudit(this FilterCollection filters)
    {
        filters.Add<Audit>(Constants.Audit);
        return filters;
    }

    public static FilterCollection AddResponseWrapper(this FilterCollection filters)
    {
        filters.Add<ResponseWrapperFilter>(Constants.ResponseWrapper);
        return filters;
    }

    public static FilterCollection AddGlobalException(this FilterCollection filters)
    {
        filters.Add<GlobalExceptionFilter>(Constants.GlobalException);
        return filters;
    }

    public static WebApplication UseDaprSecurity(this WebApplication app)
    {
        app.UseMiddleware<DaprSecurityMiddleware>();
        return app;
    }
}
