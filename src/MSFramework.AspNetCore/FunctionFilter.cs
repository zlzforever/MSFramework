using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Collections.Generic;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Audit;
using MSFramework.Domain.Event;
using MSFramework.Extensions;
using MSFramework.Function;

namespace MSFramework.AspNetCore
{
	public class FunctionFilter : ActionFilterAttribute
	{
		private const string FunctionKey = "__Function";
		private const string AuditOperationKey = "__AuditOperation";

		public FunctionFilter()
		{
			Order = 40000;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var provider = context.HttpContext.RequestServices;
			var eventBus = provider.GetService<IEventMediator>();
			// 审计通过事件放送出去，避免开销太大
			if (eventBus == null)
			{
				return;
			}

			var functionPath = context.ActionDescriptor.GetFunctionPath();
			var functionStore = provider.GetService<IFunctionStore>();
			if (functionStore == null)
			{
				throw new MSFrameworkException("未注册任何 FunctionStore");
			}

			var function = functionStore.Get(functionPath);
			if (function == null || !function.Enabled)
			{
				context.Result = new NotFoundResult();
				return;
			}

			var dict = provider.GetRequiredService<ScopedDictionary>();
			dict.TryAdd(FunctionKey, function);

			if (!function.AuditOperationEnabled)
			{
				return;
			}

			var operation = new AuditOperation
			{
				FunctionPath = function.Path,
				FunctionName = function.Name,
				Ip = context.GetClientIp(),
				UserAgent = context.HttpContext.Request.Headers["User-Agent"].FirstOrDefault(),
				CreatedTime = DateTimeOffset.Now
			};
			if (context.HttpContext.User?.Identity != null && context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				operation.UserId = identity.GetUserId();
				operation.UserName = identity.GetUserName();
				operation.NickName = identity.GetNickName();
			}

			dict.TryAdd(AuditOperationKey, operation);
		}

		public override void OnResultExecuted(ResultExecutedContext context)
		{
			var provider = context.HttpContext.RequestServices;
			var eventBus = provider.GetService<IEventMediator>();
			// 审计通过事件放送出去，避免开销太大
			if (eventBus == null)
			{
				return;
			}

			var dict = provider.GetService<ScopedDictionary>();
			if (dict.TryGetValue(AuditOperationKey, out dynamic auditOperation))
			{
				if (auditOperation.FunctionName == null)
				{
					return;
				}

				auditOperation.EndedTime = DateTimeOffset.Now;
				auditOperation.Elapsed =
					(int) (auditOperation.EndedTime - auditOperation.CreatedTime)
					.TotalMilliseconds;

				eventBus.PublishAsync(new AuditOperationEvent(auditOperation)).GetAwaiter().GetResult();
			}
		}
	}
}