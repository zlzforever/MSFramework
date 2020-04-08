using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Common;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionInitializer : Initializer
	{
		public override void Initialize(IServiceProvider serviceProvider)
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
			if (!cerberusClient.ExistsAsync(options.CerberusServiceId).Result)
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

			var permissionsExistsDict = cerberusClient.GetPermissionsAsync(options.CerberusServiceId).Result
				.ToDictionary(x => x.Identification, x => x);

			var renewalIds = new List<string>();

			foreach (var kv in permissionsInAppDict)
			{
				var permission = kv.Value;
				if (!permissionsExistsDict.ContainsKey(permission.Identification))
				{
					cerberusClient.AddPermissionAsync(options.CerberusServiceId, permission).GetAwaiter()
						.GetResult();
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
				cerberusClient.RenewalAsync(options.CerberusServiceId, string.Join(",", renewalIds)).GetAwaiter()
					.GetResult();
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
				cerberusClient.ExpireAsync(options.CerberusServiceId, string.Join(",", expiredIds)).GetAwaiter()
					.GetResult();
			}
		}
	}
}