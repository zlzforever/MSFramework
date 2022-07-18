using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Extensions;

public static class ActionDescriptorExtensions
{
    private static readonly ConcurrentDictionary<MethodInfo, (string Name, string Description)> FeatureCache =
        new();

    public static bool HasAttribute<T>(this ActionExecutingContext context) where T : Attribute
    {
        var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
        var ignoreAuditAttribute = controllerAction.MethodInfo.GetCustomAttribute<T>();
        return ignoreAuditAttribute != null;
    }
}
