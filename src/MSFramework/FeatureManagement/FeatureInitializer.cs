using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.FeatureManagement
{
	[IgnoreRegister]
	public class FeatureInitializer : InitializerBase
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var repository = serviceProvider.GetService<IFeatureRepository>();
			if (repository == null || !repository.IsAvailable())
			{
				throw new MicroserviceFrameworkException("Feature 仓储不可用");
			}

			var functionFinder = serviceProvider.GetService<IFeatureFinder>();
			if (functionFinder == null)
			{
				throw new MicroserviceFrameworkException("没有配置 IFeatureFinder");
			}

			var functionsInApp = functionFinder.GetAllList();

			var functionsInAppDict = new Dictionary<string, Feature>();
			foreach (var function in functionsInApp)
			{
				if (!functionsInAppDict.ContainsKey(function.Name))
				{
					functionsInAppDict.Add(function.Name, function);
				}
				else
				{
					throw new MicroserviceFrameworkException($"There are same functions: {function.Name}");
				}
			}

			var functionsInDatabaseDict = repository.GetAllList()
				.ToDictionary(x => x.Name, x => x);

			// 添加新功能
			foreach (var kv in functionsInAppDict)
			{
				var function = kv.Value;
				if (!functionsInDatabaseDict.ContainsKey(function.Name))
				{
					await repository.AddAsync(function);
				}
				else
				{
					if (function.Expired)
					{
						function.Renewal();
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
				}
			}

			await serviceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
		}
	}
}