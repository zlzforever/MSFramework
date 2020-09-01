using System;
using System.Collections.Generic;
using System.Reflection;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.AccessControl
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
			var scope = _services.CreateScope();
			var actionDescriptorCollectionProvider =
				scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
			var apiInfos = new List<ApiInfo>();
			var applicationInfo = scope.ServiceProvider.GetRequiredService<ApplicationInfo>();
			if (string.IsNullOrWhiteSpace(applicationInfo.Name))
			{
				throw new MicroserviceFrameworkException("Application name is not config");
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