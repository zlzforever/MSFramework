using System;
using System.Collections.Concurrent;
using System.Reflection;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.Utilities;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Extensions
{
	public static class ActionDescriptorExtensions
	{
		private static readonly ConcurrentDictionary<MethodInfo, (string Name, string Description)> FeatureCache =
			new();

		public static bool HasAttribute<T>(this ActionExecutingContext context) where T : Attribute
		{
			var controllerAction = (ControllerActionDescriptor) context.ActionDescriptor;
			var ignoreAuditAttribute = controllerAction.MethodInfo.GetCustomAttribute<T>();
			return ignoreAuditAttribute != null;
		}

		public static (string Name, string Description) GetFeature(this ActionDescriptor actionDescriptor)
		{
			var controllerAction = (ControllerActionDescriptor) actionDescriptor;
			return FeatureCache.GetOrAdd(controllerAction.MethodInfo, info =>
			{
				var action = info.Name;
				var parameters = info.GetParameters().ExpandAndToString(x =>
					$"{x.ParameterType.FullName}");
				// todo: 从 Attribute 上获取 Feature 信息
				var description = $"{controllerAction.ControllerTypeInfo.FullName}.{action}({parameters})";
				return (CryptographyUtilities.ComputeMD5(description), description);
			});
		}
	}
}