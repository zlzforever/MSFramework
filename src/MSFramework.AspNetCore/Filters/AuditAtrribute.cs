using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Audit;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Domain;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class Audit : ActionFilterAttribute
	{
		private AuditedOperation _auditedOperation;
		private IServiceProvider _serviceProvider;
		private IAuditService _auditService;

		public Audit()
		{
			Order = FilterOrders.AuditFilterOrder;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_serviceProvider = context.HttpContext.RequestServices;
			_auditService = _serviceProvider.GetRequiredService<IAuditService>();
			var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
			var applicationName = configuration["ApplicationName"];
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? Assembly.GetEntryAssembly()?.FullName
				: applicationName;
			var path = context.ActionDescriptor.GetActionPath();
			var ua = string.Join(";", context.HttpContext.Request.Headers["User-Agent"]);
			var ip = context.GetClientIp();
			_auditedOperation = new AuditedOperation(applicationName, path, ip, ua);
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				_auditedOperation.SetCreationAudited(identity.GetUserId(), identity.GetUserName());
			}
		}

		public override void OnResultExecuted(ResultExecutedContext context)
		{
			var unitOfWorkManager = _serviceProvider.GetService<IUnitOfWorkManager>();
			if (unitOfWorkManager != null)
			{
				var entities = new List<AuditedEntity>();
				foreach (var unitOfWork in unitOfWorkManager.GetUnitOfWorks())
				{
					entities.AddRange(unitOfWork.GetAuditEntities());
				}

				_auditedOperation.AddEntities(entities);
			}

			_auditedOperation.End();
			_auditService.Save(_auditedOperation);
		}
	}
}