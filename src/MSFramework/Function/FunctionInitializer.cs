using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Function
{
	public class FunctionInitializer : InitializerBase
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var logger = serviceProvider.GetRequiredService<ILogger<FunctionInitializer>>();
			var repository = serviceProvider.GetService<IFunctionDefineRepository>();
			if (!repository.IsAvailable())
			{
				logger.LogInformation("Function 仓储不可用");
				return;
			}

			var functionFinder = serviceProvider.GetService<IFunctionDefineFinder>();
			if (functionFinder == null)
			{
				logger.LogInformation("没有配置 Function 中间件");
				return;
			}

			var functionsInApp = functionFinder.GetAllList();

			var functionsInAppDict = new Dictionary<string, FunctionDefine>();
			foreach (var function in functionsInApp)
			{
				if (!functionsInAppDict.ContainsKey(function.Code))
				{
					functionsInAppDict.Add(function.Code, function);
				}
				else
				{
					throw new MicroserviceFrameworkException($"There are same functions: {function.Code}");
				}
			}

			var functionsInDatabaseDict = repository.GetAllList()
				.ToDictionary(x => x.Code, x => x);

			// 添加新功能
			foreach (var kv in functionsInAppDict)
			{
				var function = kv.Value;
				if (!functionsInDatabaseDict.ContainsKey(function.Code))
				{
					function.SetCreation("System", "System");
					await repository.InsertAsync(function);
				}
				else
				{
					if (function.Expired)
					{
						function.Renewal();
						function.SetModification("System", "System");
						await repository.UpdateAsync(function);
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
					function.SetModification("System", "System");
					await repository.UpdateAsync(function);
				}
			}

			await serviceProvider.GetRequiredService<UnitOfWorkManager>().CommitAsync();
		}
	}
}