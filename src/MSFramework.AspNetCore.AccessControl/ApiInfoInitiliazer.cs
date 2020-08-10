using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Initializers;
using MSFramework.Shared;

namespace MSFramework.AspNetCore.AccessControl
{
	public class ApiInfoInitializer : Initializer
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var accessClient = serviceProvider.GetService<IAccessClient>();
			if (accessClient == null)
			{
				return;
			}

			var options = serviceProvider.GetService<AccessControlOptions>();
			if (options == null)
			{
				return;
			}

			var apiInfoFinder = serviceProvider.GetRequiredService<AspNetCoreApiInfoFinder>();

			var localApiInfos = apiInfoFinder.GetAllList();

			var localApiInfoDict = new Dictionary<string, ApiInfo>();
			foreach (var apiInfo in localApiInfos)
			{
				if (!localApiInfoDict.ContainsKey(apiInfo.Name))
				{
					localApiInfoDict.Add(apiInfo.Name, apiInfo);
				}
				else
				{
					throw new MSFrameworkException(
						$"There are more than one access control named: {apiInfo.Name} in local");
				}
			}

			var applicationInfo = serviceProvider.GetRequiredService<ApplicationInfo>();

			if (string.IsNullOrWhiteSpace(applicationInfo.Name))
			{
				throw new MSFrameworkException("Application name is not config");
			}

			var remoteApiInfos = await accessClient.GetAllListAsync(applicationInfo.Name);
			var remoteApiInfoDict = new Dictionary<string, ApiInfo>();

			foreach (var group in remoteApiInfos)
			{
				foreach (var apiInfo in group.Value)
				{
					if (!remoteApiInfoDict.ContainsKey(apiInfo.Name))
					{
						remoteApiInfoDict.Add(apiInfo.Name, apiInfo);
					}
					else
					{
						throw new MSFrameworkException(
							$"There are more than one access control named: {apiInfo.Name} in remote");
					}
				}
			}

			foreach (var kv in localApiInfoDict)
			{
				var permission = kv.Value;
				if (!remoteApiInfoDict.ContainsKey(permission.Name))
				{
					await accessClient.CreateAsync(permission);
				}
				else
				{
					if (remoteApiInfoDict[permission.Name].Obsoleted)
					{
						await accessClient.RenewalAsync(remoteApiInfoDict[permission.Name].Id);
					}
				}
			}

			// 标记功能过期
			foreach (var kv in remoteApiInfoDict)
			{
				var apiInfo = kv.Value;
				if (!localApiInfoDict.ContainsKey(kv.Key))
				{
					await accessClient.ObsoleteAsync(apiInfo.Id);
				}
			}
		}
	}
}