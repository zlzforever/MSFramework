using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.Function
{
	public class FunctionInitializer : Initializer
	{
		public override void Initialize(IServiceProvider serviceProvider)
		{
			var scope = serviceProvider.CreateScope();
			var functionFinder = scope.ServiceProvider.GetService<IFunctionFinder>();
			var logger = scope.ServiceProvider.GetRequiredService<ILogger<FunctionInitializer>>();
			if (functionFinder == null)
			{
				logger.LogInformation("没有配置 Function 中间件");
				return;
			}

			var functionInApp = functionFinder.GetAllList().ToDictionary(x => x.Path, x => x);
			var store = scope.ServiceProvider.GetService<IFunctionStore>();
			var functionsInDatabase = store.GetAllList().ToDictionary(x => x.Path, x => x);

			// 添加新功能
			foreach (var kv in functionInApp)
			{
				var function = kv.Value;
				if (!functionsInDatabase.ContainsKey(function.Path))
				{
					function.SetCreationAudited("System", "System");
					store.Add(function);
				}
				else
				{
					if (function.Expired)
					{
						function.Renewal();
						function.SetModificationAudited("System", "System");
						store.Update(function);
					}
				}
			}

			// 标记功能过期
			foreach (var kv in functionsInDatabase)
			{
				var function = kv.Value;
				if (!functionInApp.ContainsKey(kv.Key))
				{
					function.Expire();
					function.SetModificationAudited("System", "System");
					store.Update(function);
				}
			}

			scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().CommitAsync().GetAwaiter();
			scope.Dispose();
		}
	}
}