using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Audits;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class Audit : ActionFilterAttribute
	{
		private AuditOperation _auditedOperation;
		private ILogger _logger;
		private IAuditService _auditService;
		private IUnitOfWorkManager _auditUnitOfWorkManager;

		public Audit()
		{
			Order = FilterOrders.Audit;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var scope = context.HttpContext.RequestServices.CreateScope();

			_logger = scope.ServiceProvider.GetRequiredService<ILogger<Audit>>();
			_logger.LogDebug("Executing audit filter");

			_auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
			if (_auditService == null)
			{
				throw new MicroserviceFrameworkException("AuditService is not registered");
			}

			_auditUnitOfWorkManager = scope.ServiceProvider.GetService<IUnitOfWorkManager>();

			var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
			var applicationName = configuration["ApplicationName"];
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? Assembly.GetEntryAssembly()?.FullName
				: applicationName;
			var path = context.ActionDescriptor.GetActionPath();
			var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
			var ip = context.GetRemoteIpAddress();
			_auditedOperation = new AuditOperation(applicationName, path, ip, ua);
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				_auditedOperation.SetCreation(identity.GetUserId(), identity.GetUserName());
			}
			else
			{
				_auditedOperation.SetCreation("Anonymous", "Anonymous");
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			var currentUnitOfWorkManager = context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			if (currentUnitOfWorkManager != null)
			{
				var entities = new List<AuditEntity>();
				foreach (var unitOfWork in currentUnitOfWorkManager.GetUnitOfWorks())
				{
					entities.AddRange(unitOfWork.GetAuditEntities());
				}

				_auditedOperation.AddEntities(entities);
			}

			_auditedOperation.End();

			_auditService.Save(_auditedOperation);
			_auditUnitOfWorkManager.Commit();

			_logger.LogDebug("Executed audit filter");
		}
	}
}