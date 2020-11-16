using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Initializer;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	public class ApiInfoInitializer : InitializerBase
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			await new Cerberus.AspNetCore.AccessControl.ApiInfoInitializer().InitializeAsync(serviceProvider);
		}
	}
}