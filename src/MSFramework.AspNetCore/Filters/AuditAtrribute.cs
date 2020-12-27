using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Filters
{
	/// <summary>
	/// todo: 审计是否应该和请求相同一个 scope
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class Audit : ActionFilterAttribute
	{
		public Audit()
		{
			Order = FilterOrders.Audit;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.HasAttribute<IgnoreAudit>())
			{
				await base.OnActionExecutionAsync(context, next);
				return;
			}

			using var scope = context.HttpContext.RequestServices.CreateScope();
			var serviceProvider = scope.ServiceProvider;

			var auditService = serviceProvider.GetRequiredService<IAuditService>();
			if (auditService == null)
			{
				throw new MicroserviceFrameworkException("AuditService is not registered");
			}

			var configuration = serviceProvider.GetRequiredService<IConfiguration>();
			var applicationName = configuration["ApplicationName"];
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? Assembly.GetEntryAssembly()?.FullName
				: applicationName;
			var path = context.ActionDescriptor.GetActionPath();
			var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
			var ip = context.GetRemoteIpAddress();
			var url = context.HttpContext.Request.GetDisplayUrl();
			var auditedOperation = new AuditOperation(applicationName, path, url, ip, ua);
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				auditedOperation.SetCreation(identity.GetUserId(), identity.GetUserName());
			}
			else
			{
				auditedOperation.SetCreation("Anonymous", "Anonymous");
			}

			await base.OnActionExecutionAsync(context, next);

			// comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
			var uowManager = context.HttpContext.RequestServices.GetService<UnitOfWorkManager>();
			if (uowManager != null)
			{
				var entities = new List<AuditEntity>();
				foreach (var unitOfWork in uowManager.GetUnitOfWorks())
				{
					entities.AddRange(unitOfWork.GetAuditEntities());
				}

				auditedOperation.AddEntities(entities);
			}

			auditedOperation.End();
			await auditService.SaveAsync(auditedOperation);
		}
	}
}