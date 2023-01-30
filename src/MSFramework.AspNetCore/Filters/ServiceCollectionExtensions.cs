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

    public static FilterCollection AddGlobalException(this FilterCollection filters)
    {
        filters.Add<GlobalExceptionFilter>();
        return filters;
    }

    // public static FilterCollection AddActionException(this FilterCollection filters)
    // {
    //     filters.Add<ActionExceptionFilter>();
    //     return filters;
    // }
}
