using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.DependencyInjection;
using MSFramework.Shared;

namespace MSFramework.AspNetCore.AccessControl
{
	public class AspNetCoreApiInfoFinder : ISingletonDependency
	{
		private readonly IServiceProvider _services;

		public AspNetCoreApiInfoFinder(IServiceProvider serviceProvider)
		{
			_services = serviceProvider;
		}

		public List<ApiInfo> GetAllList()
		{
			var actionDescriptorCollectionProvider =
				_services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var apiInfos = new List<ApiInfo>();
			var applicationInfo = _services.GetRequiredService<ApplicationInfo>();
			if (string.IsNullOrWhiteSpace(applicationInfo.Name))
			{
				throw new MSFrameworkException("Application name is not config");
			}

			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				if (actionDescriptor is ControllerActionDescriptor descriptor)
				{
					var attribute = descriptor.MethodInfo.GetCustomAttribute<AccessControlAttribute>();
					if (attribute != null)
					{
						var apiInfo = new ApiInfo
						{
							Name = attribute.Name,
							Description = attribute.Description,
							Application = applicationInfo.Name,
							Group = attribute.Group
						};
						apiInfos.Add(apiInfo);
					}
				}
			}

			return apiInfos;
		}
	}
}