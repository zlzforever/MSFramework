using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Common;
using MSFramework.Domain;

namespace MSFramework.Function
{
	public class FunctionInitializer : Initializer
	{
		public override void Initialize(IServiceProvider serviceProvider)
		{
			var functionFinder = serviceProvider.GetService<IFunctionFinder>();
			var logger = serviceProvider.GetRequiredService<ILogger<FunctionInitializer>>();
			if (functionFinder == null)
			{
				logger.LogInformation("没有配置 Function 中间件");
				return;
			}

			var functionsInApp = functionFinder.GetAllList();

			var functionsInAppDict = new Dictionary<string, Function>();
			foreach (var function in functionsInApp)
			{
				if (!functionsInAppDict.ContainsKey(function.Path))
				{
					functionsInAppDict.Add(function.Path, function);
				}
				else
				{
					throw new MSFrameworkException($"There are same route apis: {function.Path}");
				}
			}

			var store = serviceProvider.GetService<IFunctionStore>();
			var functionsInDatabaseDict = store.GetAllList().ToDictionary(x => x.Path, x => x);

			// 添加新功能
			foreach (var kv in functionsInAppDict)
			{
				var function = kv.Value;
				if (!functionsInDatabaseDict.ContainsKey(function.Path))
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
			foreach (var kv in functionsInDatabaseDict)
			{
				var function = kv.Value;
				if (!functionsInAppDict.ContainsKey(kv.Key))
				{
					function.Expire();
					function.SetModificationAudited("System", "System");
					store.Update(function);
				}
			}

			serviceProvider.GetRequiredService<IUnitOfWorkManager>().CommitAsync().GetAwaiter();
		}
	}
}