using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Permission.Extensions;

namespace MSFramework.AspNetCore.Permission
{
	public class AspNetCorePermissionFinder
	{
		private readonly IServiceProvider _services;

		public AspNetCorePermissionFinder(IServiceProvider serviceProvider)
		{
			_services = serviceProvider;
		}

		public List<Permission> GetAllList()
		{
			var actionDescriptorCollectionProvider =
				_services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var permissions = new List<Permission>();
			var options = _services.GetRequiredService<PermissionOptions>();
			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				if (actionDescriptor is ControllerActionDescriptor descriptor)
				{
					if (descriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>() != null ||
					    descriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>() != null)
					{
						var permission = actionDescriptor.GetPermission();
						permission.Service = options.Service;
						permissions.Add(permission);
					}
				}
			}

			return permissions;
		}
	}
}