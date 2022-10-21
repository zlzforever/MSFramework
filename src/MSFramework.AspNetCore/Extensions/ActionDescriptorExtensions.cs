using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Extensions;

public static class ActionDescriptorExtensions
{
    public static bool HasAttribute<T>(this ActionExecutingContext context) where T : Attribute
    {
        var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
        var ignoreAuditAttribute = controllerAction.MethodInfo.GetCustomAttribute<T>();
        return ignoreAuditAttribute != null;
    }
}
