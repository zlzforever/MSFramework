using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///     Action 执行上下文特性检测的扩展方法
/// </summary>
public static class ActionDescriptorExtensions
{
    /// <param name="context"></param>
    extension(ActionExecutingContext context)
    {
        /// <summary>
        ///     判断 Action 方法是否标记了指定类型的特性
        /// </summary>
        /// <typeparam name="T">要检测的特性类型</typeparam>
        /// <returns>存在返回 true</returns>
        public bool HasAttribute<T>() where T : Attribute
        {
            var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
            var ignoreAuditAttribute = controllerAction.MethodInfo.GetCustomAttribute<T>();
            return ignoreAuditAttribute != null;
        }

        /// <summary>
        ///     通过全名称判断 Action 方法是否标记了指定类型的特性
        /// </summary>
        /// <param name="type">特性类型的 FullName</param>
        /// <returns>存在返回 true</returns>
        public bool HasAttribute(string type)
        {
            var controllerAction = (ControllerActionDescriptor)context.ActionDescriptor;
            var attributes = controllerAction.MethodInfo.GetCustomAttributes();
            var has = attributes.Any(x => x.GetType().FullName == type);
            return has;
        }
    }
}
