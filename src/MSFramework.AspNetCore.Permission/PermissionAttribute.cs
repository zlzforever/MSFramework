using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

					if (options.UseSession)
					{
						if (string.IsNullOrWhiteSpace(options.Authority))
						{
							throw new ApplicationException("Authority is missing in configuration");
						}

						var httpClientFactory =
							context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
						var httpClient = httpClientFactory.CreateClient("Session");
						var token = await context.HttpContext.GetTokenAsync("access_token");
						httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
						var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head,
							$"{options.Authority}/connect/online-session"));
						if (response.StatusCode != HttpStatusCode.OK)
						{
							context.HttpContext.Response.StatusCode = 401;
							return;
						}
					}

					var cerberusClient =
						context.HttpContext.RequestServices.GetRequiredService<ICerberusClient>();
					var identification = descriptor.GetActionPath();
					var userId = context.HttpContext.RequestServices.GetRequiredService<ISession>().UserId;
					var hasPermission =
						await cerberusClient.HasPermissionAsync(userId, options.CerberusServiceId, identification);
					if (!hasPermission)
					{
						await context.HttpContext.ForbidAsync();
						return;
					}
					else
					{
						// todo:
					}
				}
			}

			await next();
		}
	}
}