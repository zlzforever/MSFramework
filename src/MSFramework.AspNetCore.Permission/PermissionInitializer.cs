using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Initializers;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionInitializer : Initializer
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var options = serviceProvider.GetService<PermissionOptions>();
			if (options == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(options.CerberusSecurityHeader))
			{
				throw new ApplicationException("CerberusSecurityHeader is missing");
			}

			var cerberusClient = serviceProvider.GetRequiredService<ICerberusClient>();
			if (!await cerberusClient.ExistsAsync(options.CerberusServiceId))
			{
				throw new ApplicationException(
					$"Service {options.CerberusServiceId} not exists in cerberus or your config is not correct, please create it firstly");
			}

			var permissionFinder = serviceProvider.GetRequiredService<AspNetCorePermissionFinder>();

			var permissionsInApp = permissionFinder.GetAllList();

			var permissionsInAppDict = new Dictionary<string, Permission>();
			foreach (var permission in permissionsInApp)
			{
				if (!permissionsInAppDict.ContainsKey(permission.Identification))
				{
					permissionsInAppDict.Add(permission.Identification, permission);
				}
				else
				{
					throw new MSFrameworkException($"There are same permission: {permission.Identification}");
				}
			}

			var permissionsExistsDict = (await cerberusClient.GetPermissionsAsync(options.CerberusServiceId))
				.ToDictionary(x => x.Identification, x => x);

			var renewalIds = new List<string>();

			foreach (var kv in permissionsInAppDict)
			{
				var permission = kv.Value;
				if (!permissionsExistsDict.ContainsKey(permission.Identification))
				{
					await cerberusClient.AddPermissionAsync(options.CerberusServiceId, permission);
				}
				else
				{
					if (permissionsExistsDict[permission.Identification].Expired)
					{
						renewalIds.Add(permissionsExistsDict[permission.Identification].Id);
					}
				}
			}

			if (renewalIds.Count > 0)
			{
				await cerberusClient.RenewalAsync(options.CerberusServiceId, string.Join(",", renewalIds));
			}

			var expiredIds = new List<string>();
			// 标记功能过期
			foreach (var kv in permissionsExistsDict)
			{
				var permission = kv.Value;
				if (!permissionsInAppDict.ContainsKey(kv.Key))
				{
					expiredIds.Add(permission.Id);
				}
			}

			if (expiredIds.Count > 0)
			{
				await cerberusClient.ExpireAsync(options.CerberusServiceId, string.Join(",", expiredIds));
			}
		}
	}
}