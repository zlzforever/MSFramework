using System;
using System.Collections.Generic;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Function;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Function
{
	public class AspNetCoreFunctionFinder : IFunctionDefineFinder
	{
		private readonly IServiceProvider _services;

		public AspNetCoreFunctionFinder(IServiceProvider serviceProvider)
		{
			_services = serviceProvider;
		}

		public IEnumerable<FunctionDefine> GetAllList()
		{
			var actionDescriptorCollectionProvider =
				_services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var functions = new List<FunctionDefine>();
			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				functions.Add(actionDescriptor.GetFunction());
			}

			return functions;
		}
	}
}