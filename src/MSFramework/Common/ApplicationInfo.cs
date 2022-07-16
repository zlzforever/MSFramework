using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework.Common
{
	public class ApplicationInfo
	{
		public string Name { get; }

		public ApplicationInfo(IHostEnvironment hostEnvironment, IConfiguration configuration)
		{
			var applicationName = configuration["ApiName"];
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? configuration["ApplicationName"]
				: applicationName;
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? hostEnvironment.ApplicationName
				: applicationName;
			applicationName = string.IsNullOrWhiteSpace(applicationName)
				? Assembly.GetEntryAssembly()?.FullName
				: applicationName;
			Name = applicationName;
		}
	}
}