using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Extensions
{
	public static class ActionDescriptorExtensions
	{
		public static MSFramework.Function.FunctionDefine GetFunction(this ActionDescriptor actionDescriptor)
		{
			var controllerAction = (ControllerActionDescriptor) actionDescriptor;
			var functionPath = actionDescriptor.GetActionPath();
			var parameters = controllerAction.Parameters.Select(x => $"{x.ParameterType.Name} {x.Name}")
				.ExpandAndToString();
			var name =
				$"{controllerAction.MethodInfo.DeclaringType.FullName}.{controllerAction.MethodInfo.Name}({parameters})";
			return new MSFramework.Function.FunctionDefine(name, functionPath, null);
		}

		public static string GetActionPath(this ActionDescriptor actionDescriptor)
		{
			var controllerAction = (ControllerActionDescriptor) actionDescriptor;

			var httpMethodMetadata = controllerAction.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata);

			var methods = new List<string>();
			if (httpMethodMetadata != null)
			{
				methods.AddRange(((HttpMethodMetadata) httpMethodMetadata).HttpMethods);
			}

			if (controllerAction.AttributeRouteInfo == null)
			{
				var area = actionDescriptor.GetAreaName();
				var controller = actionDescriptor.GetControllerName();
				var action = actionDescriptor.GetActionName();
				var path = (area == null ? $"{controller}/{action}" : $"{area}/{controller}/{action}").ToLower();
				return $"{methods.ExpandAndToString()} {path}".ToLower();
			}
			else
			{
				var regex = new Regex("{\\w+}");
				var template = controllerAction.AttributeRouteInfo.Template.ToLower();
				template = regex.Replace(template, "{arg}");
				return $"{methods.ExpandAndToString()} {template}".ToLower();;
			}
		}

		/// <summary>
		/// 获取Area名
		/// </summary>
		public static string GetAreaName(this ActionDescriptor actionDescriptor)
		{
			actionDescriptor.RouteValues.TryGetValue("area", out var area);
			return area;
		}

		/// <summary>
		/// 获取Controller名
		/// </summary>
		public static string GetControllerName(this ActionDescriptor actionDescriptor)
		{
			actionDescriptor.RouteValues.TryGetValue("controller", out var area);
			return area;
		}

		/// <summary>
		/// 获取Action名
		/// </summary>
		public static string GetActionName(this ActionDescriptor actionDescriptor)
		{
			actionDescriptor.RouteValues.TryGetValue("action", out var area);
			return area;
		}
	}
}