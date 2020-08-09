using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSFramework.Application;
using MSFramework.Shared;

namespace MSFramework.AspNetCore.AccessControl
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class AccessControlAttribute : ActionFilterAttribute
	{
		private static readonly Type AttributeType = typeof(AccessControlAttribute);

		/// <summary>
		/// 权限名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 访问此资源的访问者类型
		/// </summary>
		public SubjectType SubjectType { get; private set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string Description { get; private set; }

		public AccessControlAttribute(string name, SubjectType subjectType = SubjectType.Role, string description = null)
		{
			Check.NotEmpty(name, nameof(name));
			Name = name;
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

				var permission1 = descriptor.ControllerTypeInfo.GetCustomAttribute(AttributeType);
				var permission2 = descriptor.MethodInfo.GetCustomAttribute(AttributeType);

				if (permission1 == null && permission2 == null)
				{
					await next();
				}
				else
				{
					var permission = (AccessControlAttribute) (permission2 ?? permission1);

					var session = context.HttpContext.RequestServices.GetRequiredService<ISession>();
					var hostingEnvironment =
						context.HttpContext.RequestServices.GetRequiredService<IHostingEnvironment>();

					var subject = permission.SubjectType == SubjectType.Identity
						? session.UserId
						: session.Roles == null
							? null
							: string.Join(",", session.Roles);

					if (string.IsNullOrWhiteSpace(subject))
					{
						// todo: 401
						await context.HttpContext.ForbidAsync();
						return;
					}

					var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
					var applicationName = configuration["ApplicationName"];
					applicationName = string.IsNullOrWhiteSpace(applicationName)
						? hostingEnvironment.ApplicationName
						: applicationName;
					applicationName = string.IsNullOrWhiteSpace(applicationName)
						? Assembly.GetEntryAssembly()?.FullName
						: applicationName;

					if (string.IsNullOrWhiteSpace(applicationName))
					{
						throw new MSFrameworkException("ApplicationName is empty");
					}

					var tuple =
						await accessClient.HasAccessAsync(subject, permission.Name, "full", applicationName);
					if (!tuple.HasAccess)
					{
						//todo:
						switch (tuple.StatusCode)
						{
							case HttpStatusCode.Forbidden:
							{
								await context.HttpContext.ForbidAsync();
								return;
							}
							case HttpStatusCode.NotFound:
							{
								context.Result = new NotFoundResult();
								return;
							}
							default:
							{
								context.Result = new NotFoundResult();
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