using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Filters
{
	/// <summary>
	///  
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
			IAuditStore auditStore = null;
			AuditOperation auditedOperation = null;
			if (Conts.MethodDict.ContainsKey(context.HttpContext.Request.Method))
			{
				// 必须保证审计和业务用的是不同的 DbContext 不然，会导致数据异常入库
				using var scope = context.HttpContext.RequestServices.CreateScope();
				auditStore = scope.ServiceProvider.GetService<IAuditStore>();
				if (auditStore == null)
				{
					throw new MicroserviceFrameworkException("AuditStore is not registered");
				}

				var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
				var ip = context.GetRemoteIpAddress();
				var url = context.HttpContext.Request.GetDisplayUrl();
				var deviceId = context.HttpContext.Request.Query["deviceId"].ToString();
				var deviceModel = context.HttpContext.Request.Query["deviceId"].ToString();
				var lat = context.HttpContext.Request.Query["lat"].ToString();
				var lng = context.HttpContext.Request.Query["lng"].ToString();
				auditedOperation = new AuditOperation(
					$"{context.HttpContext.Request.Method} {url}", ua, ip, deviceModel, deviceId,
					double.TryParse(lat, out var a) ? a : null, double.TryParse(lng, out var n) ? n : null);

				auditedOperation.SetCreation(
					context.HttpContext.User.Identity is { IsAuthenticated: true } and ClaimsIdentity identity
						? identity.GetUserId()
						: string.Empty);

				context.HttpContext.Items.Add("___AuditOperation", auditedOperation);
			}

			await base.OnActionExecutionAsync(context, next);

			// comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
			// comment: 只有有变化的数据才会尝试获取变更对象
			if (auditStore != null &&
			    Conts.MethodDict.ContainsKey(context.HttpContext.Request.Method))
			{
				var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
				if (unitOfWork != null)
				{
					auditedOperation.AddEntities(unitOfWork.GetAuditEntities());
				}

				auditedOperation.End();
				await auditStore.AddAsync(auditedOperation);
				await auditStore.FlushAsync();
			}
		}
	}
}