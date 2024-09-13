using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///
/// </summary>
public static class ActionDescriptorExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool HasAttribute<T>(this ActionExecutingContext context) where T : Attribute
    {
        var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
        var ignoreAuditAttribute = controllerAction.MethodInfo.GetCustomAttribute<T>();
        return ignoreAuditAttribute != null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool HasAttribute(this ActionExecutingContext context, string type)
    {
        var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
        var attributes = controllerAction.MethodInfo.GetCustomAttributes();
        var has = attributes.Any(x => x.GetType().FullName == type);
        return has;
    }
}
