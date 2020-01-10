using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Permission.Application;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionHandler : IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
			{
				// 如果控制器上标了 Pay 则表达所有 action 都需要做支付校验
				if (descriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>() != null ||
				    descriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>() != null)
				{
					context.HttpContext.RequestServices.GetRequiredService<IPermissionChecker>();
				}
			}

			await next();
		}
	}
}