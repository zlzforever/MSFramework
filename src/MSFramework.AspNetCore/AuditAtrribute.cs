using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Audit;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class AuditAttribute : ActionFilterAttribute
	{
		public override void OnResultExecuted(ResultExecutedContext context)
		{
			var provider = context.HttpContext.RequestServices;
			var logger = provider.GetRequiredService<ILogger<AuditAttribute>>();
			var auditService = provider.GetService<IAuditService>();
			if (auditService == null)
			{
				logger.LogWarning("AuditService is missing");
				return;
			}
			var unitOfWorkManager = provider.GetService<IUnitOfWorkManager>();
			if (unitOfWorkManager != null)
			{
				var auditOperation=new AuditOperation();
				foreach (var unitOfWork in unitOfWorkManager.GetAllUnitOfWorks())
				{
					auditOperation.Entities.AddRange(unitOfWork.GetAuditEntries());
				}

				auditService.Save(auditOperation);
			}
			if (auditOperation.FunctionName == null)
			{
				return;
			}

			auditOperation.EndedTime = DateTimeOffset.Now;
			auditOperation.Elapsed =
				(int) (auditOperation.EndedTime - auditOperation.CreatedTime)
				.TotalMilliseconds;

		}
	}
}