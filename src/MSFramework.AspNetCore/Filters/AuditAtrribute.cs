using System;
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
			Order = Conts.Audit;
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

			// 必须保证审计和业务用的是不同的 DbContext 不然，会导致数据异常入库
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
			var feature = context.ActionDescriptor.GetFeature().Description;
			var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
			var ip = context.GetRemoteIpAddress();
			var url = context.HttpContext.Request.GetDisplayUrl();
			var auditedOperation = new AuditOperation(applicationName, feature,
				$"{context.HttpContext.Request.Method} {url}", ip, ua);
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				auditedOperation.SetCreation(identity.GetUserId(), identity.GetUserName());
			}
			else
			{
				auditedOperation.SetCreation(string.Empty, string.Empty);
			}

			context.HttpContext.Items.Add("AuditOperation", auditedOperation);

			await base.OnActionExecutionAsync(context, next);

			// comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
			// comment: 只有有变化的数据才会尝试获取变更对象
			if (Conts.MethodDict.ContainsKey(context.HttpContext.Request.Method))
			{
				var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
				if (unitOfWork != null)
				{
					auditedOperation.AddEntities(unitOfWork.GetAuditEntities());
				}
			}

			auditedOperation.End();
			await auditService.AddAsync(auditedOperation);
		}
	}
}