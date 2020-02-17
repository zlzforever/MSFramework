using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore.Function;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Function;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionInitializer : Initializer
	{
		public override void Initialize(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();

			var options = scope.ServiceProvider.GetRequiredService<PermissionOptions>();
			if (string.IsNullOrWhiteSpace(options.Service))
			{
				throw new ApplicationException("Audience or Service is missing");
			}

			if (string.IsNullOrWhiteSpace(options.SecurityHeader))
			{
				throw new ApplicationException("CerberusSecurityHeader is missing");
			}

			var cerberusClient = scope.ServiceProvider.GetRequiredService<CerberusClient>();
			cerberusClient.CreateService(options.Service).GetAwaiter().GetResult();

			var permissionFinder = scope.ServiceProvider.GetRequiredService<AspNetCorePermissionFinder>();

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

			var permissionsExistsDict = cerberusClient.GetPermissionsAsync(options.Service).Result
				.ToDictionary(x => x.Identification, x => x);

			var renewalIds = new List<string>();

			foreach (var kv in permissionsInAppDict)
			{
				var permission = kv.Value;
				if (!permissionsExistsDict.ContainsKey(permission.Identification))
				{
					cerberusClient.AddPermissionAsync(permission).GetAwaiter()
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
				cerberusClient.RenewalAsync(string.Join(",", renewalIds)).GetAwaiter().GetResult();
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
				cerberusClient.ExpireAsync(string.Join(",", expiredIds)).GetAwaiter().GetResult();
			}
		}
	}
}