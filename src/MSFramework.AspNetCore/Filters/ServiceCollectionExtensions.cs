using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

public static class ServiceCollectionExtensions
{
    [Obsolete]
    public static FilterCollection AddUnitOfWork(this FilterCollection filters)
    {
        filters.Add<UnitOfWork>(Conts.UnitOfWork);
        return filters;
    }

    [Obsolete]
    public static FilterCollection AddAudit(this FilterCollection filters)
    {
        filters.Add<Audit>(Conts.Audit);
        return filters;
    }
}
