using System;
using System.Threading.Tasks;
using MicroserviceFramework.Initializer;

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