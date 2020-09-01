using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AccessControlAttribute : ActionFilterAttribute
	{
		private static readonly Type AttributeType = typeof(AccessControlAttribute);

		/// <summary>
		/// 权限名称
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 访问此资源的访问者类型
		/// </summary>
		public SubjectType SubjectType { get; }

		/// <summary>
		/// 分组
		/// </summary>
		public string Group { get; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string Description { get; }

		public AccessControlAttribute(string name, string group = "Default", SubjectType subjectType = SubjectType.Role,
			string description = null)
		{
			Check.NotEmpty(name, nameof(name));
			Check.NotEmpty(group, nameof(group));

			Name = name;
			Group = group;
			SubjectType = subjectType;
			Description = description;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
			{
				var accessClient =
					context.HttpContext.RequestServices.GetService<IAccessClient>();
				if (accessClient == null)
				{
					await next();
					return;
				}

				var accessControl = (AccessControlAttribute) descriptor.MethodInfo.GetCustomAttribute(AttributeType);

				if (accessControl == null)
				{
					await next();
				}
				else
				{
					var session = context.HttpContext.RequestServices.GetRequiredService<ISession>();
					var hostEnvironment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

					var subject = hostEnvironment.IsDevelopment()
						? "admin"
						: accessControl.SubjectType == SubjectType.Identity
							? session.UserId
							: session.Roles == null
								? null
								: string.Join(",", session.Roles);

					if (string.IsNullOrWhiteSpace(subject))
					{
						await context.HttpContext.ChallengeAsync();
						return;
					}

					var applicationInfo = context.HttpContext.RequestServices.GetRequiredService<ApplicationInfo>();

					if (string.IsNullOrWhiteSpace(applicationInfo.Name))
					{
						throw new MicroserviceFrameworkException("Application name is not config");
					}

					var tuple =
						await accessClient.HasAccessAsync(subject, accessControl.Name, "access", applicationInfo.Name);
					if (!tuple.HasAccess)
					{
						//todo:
						switch (tuple.StatusCode)
						{
							case HttpStatusCode.InternalServerError:
							{
								throw new MicroserviceFrameworkException("权限服务异常");
							}
							default:
							{
								await context.HttpContext.ChallengeAsync();
								return;
							}
						}
					}

					await next();
				}
			}
		}
	}
}