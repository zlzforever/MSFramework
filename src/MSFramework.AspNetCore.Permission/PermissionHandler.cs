using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Application;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionHandler : IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
			{
				if (descriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>() != null ||
				    descriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>() != null)
				{
					var options = context.HttpContext.RequestServices.GetRequiredService<PermissionOptions>();
					var cerberusClient =
						context.HttpContext.RequestServices.GetRequiredService<CerberusClient>();
					var identification = descriptor.GetFunctionPath();
					var userId = context.HttpContext.RequestServices.GetRequiredService<IMSFrameworkSession>().UserId;
					var hasPermission =
						await cerberusClient.HasPermissionAsync(userId, options.Service, identification);
					if (!hasPermission)
					{
						throw new ApplicationException("Access dined");
					}
				}
			}

			await next();
		}
	}
}