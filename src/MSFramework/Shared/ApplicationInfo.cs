using System.Reflection;
using MicroserviceFramework.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework.Shared
{
	public class ApplicationInfo : IScopeDependency
	{
		private readonly IHostEnvironment _hostEnvironment;
		private readonly IConfiguration _configuration;

		public string Name
		{
			get
			{
				var applicationName = _configuration["ApiName"];
				applicationName = string.IsNullOrWhiteSpace(applicationName)
					? _configuration["ApplicationName"]
					: applicationName;
				applicationName = string.IsNullOrWhiteSpace(applicationName)
					? _hostEnvironment.ApplicationName
					: applicationName;
				applicationName = string.IsNullOrWhiteSpace(applicationName)
					? Assembly.GetEntryAssembly()?.FullName
					: applicationName;
				return applicationName;
			}
		}

		public ApplicationInfo(IHostEnvironment hostEnvironment, IConfiguration configuration)
		{
			_hostEnvironment = hostEnvironment;
			_configuration = configuration;
		}
	}
}