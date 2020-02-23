using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Permission
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class PermissionAttribute : ActionFilterAttribute
	{
		private static readonly Type AttributeType = typeof(PermissionAttribute);

		/// <summary>
		/// 权限名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 所属模块
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string Description { get; set; }

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
			{
				if (descriptor.ControllerTypeInfo.GetCustomAttribute(AttributeType) != null ||
				    descriptor.MethodInfo.GetCustomAttribute(AttributeType) != null)
				{
					var options = context.HttpContext.RequestServices.GetRequiredService<PermissionOptions>();
					var cerberusClient =
						context.HttpContext.RequestServices.GetRequiredService<ICerberusClient>();
					var identification = descriptor.GetFunctionPath();
					var userId = context.HttpContext.RequestServices.GetRequiredService<IMSFrameworkSession>().UserId;
					var hasPermission =
						await cerberusClient.HasPermissionAsync(userId, options.CerberusServiceId, identification);
					if (!hasPermission)
					{
						await context.HttpContext.ForbidAsync();
						return;
					}
				}
			}

			await next();
		}
	}
}