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

			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				if (actionDescriptor is ControllerActionDescriptor descriptor)
				{
					if (descriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>() != null ||
					    descriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>() != null)
					{
						var permission = actionDescriptor.GetPermission();
						permissions.Add(permission);
					}
				}
			}

			return permissions;
		}
	}
}