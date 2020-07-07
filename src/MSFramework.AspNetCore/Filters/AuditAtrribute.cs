using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Audit;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Domain;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class Audit : ActionFilterAttribute
	{
		private AuditOperation _auditedOperation;
		private ILogger _logger;
		private IAuditService _auditService;

		public Audit()
		{
			Order = FilterOrders.Audit;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidModelStateFilter>>();
			_logger.LogDebug("Executing audit filter");

			_auditService = context.HttpContext.RequestServices.GetRequiredService<IAuditService>();
			if (_auditService == null)
			{
				throw new MSFrameworkException("AuditService is not registered");
			}

			var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
			var applicationName = configuration["ApplicationName"];
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? Assembly.GetEntryAssembly()?.FullName
				: applicationName;
			var path = context.ActionDescriptor.GetActionPath();
			var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
			var ip = context.GetClientIp();
			_auditedOperation = new AuditOperation(applicationName, path, ip, ua);
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				_auditedOperation.SetCreationAudited(identity.GetUserId(), identity.GetUserName());
			}
			else
			{
				_auditedOperation.SetCreationAudited("Anonymous", "Anonymous");
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			var unitOfWorkManager = context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			if (unitOfWorkManager != null)
			{
				var entities = new List<AuditEntity>();
				foreach (var unitOfWork in unitOfWorkManager.GetUnitOfWorks())
				{
					entities.AddRange(unitOfWork.GetAuditEntities());
				}

				_auditedOperation.AddEntities(entities);
			}

			_auditedOperation.End();
			_auditService.Save(_auditedOperation);

			_logger.LogDebug("Executed audit filter");
		}
	}
}