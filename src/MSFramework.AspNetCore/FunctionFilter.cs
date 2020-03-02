using System;
using System.Linq;
using System.Security.Claims;
using EventBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Collections.Generic;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Audit;
using MSFramework.Extensions;
using MSFramework.Function;

namespace MSFramework.AspNetCore
{
	public class FunctionFilter : ActionFilterAttribute
	{
		public FunctionFilter()
		{
			Order = 40000;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var provider = context.HttpContext.RequestServices;
			var eventBus = provider.GetService<IEventBus>();
			// 审计通过事件放送出去，避免开销太大
			if (eventBus == null)
			{
				return;
			}

			var functionPath = context.ActionDescriptor.GetFunctionPath();
			var function = provider.GetRequiredService<IFunctionStore>().Get(functionPath);

			if (!function.Enabled)
			{
				context.Result = new NotFoundResult();
				return;
			}

			var dict = provider.GetRequiredService<ScopedDictionary>();
			dict.Function = function;

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
			if (context.HttpContext.User.Identity.IsAuthenticated &&
			    context.HttpContext.User.Identity is ClaimsIdentity identity)
			{
				dict.Identity = identity;
				operation.UserId = identity.GetUserId();
				operation.UserName = identity.GetUserName();
				operation.NickName = identity.GetNickName();
			}

			dict.AuditOperation = operation;
		}

		public override void OnResultExecuted(ResultExecutedContext context)
		{
			var provider = context.HttpContext.RequestServices;
			var eventBus = provider.GetService<IEventBus>();
			// 审计通过事件放送出去，避免开销太大
			if (eventBus == null)
			{
				return;
			}

			var dict = provider.GetService<ScopedDictionary>();
			if (dict.AuditOperation?.FunctionName == null)
			{
				return;
			}

			dict.AuditOperation.EndedTime = DateTimeOffset.Now;
			dict.AuditOperation.Elapsed =
				(int) (dict.AuditOperation.EndedTime - dict.AuditOperation.CreatedTime)
				.TotalMilliseconds;
			
			eventBus.PublishAsync(new AuditOperationEvent(dict.AuditOperation)).GetAwaiter();
		}
	}
}