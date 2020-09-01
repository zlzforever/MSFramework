using System;
using System.Collections.Generic;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Functions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Functions
{
	public class AspNetCoreFunctionFinder : IFunctionFinder
	{
		private readonly IServiceProvider _services;

		public AspNetCoreFunctionFinder(IServiceProvider serviceProvider)
		{
			_services = serviceProvider;
		}

		public IEnumerable<Function> GetAllList()
		{
			var actionDescriptorCollectionProvider =
				_services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var functions = new List<Function>();
			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				functions.Add(actionDescriptor.GetFunction());
			}

			return functions;
		}
	}
}