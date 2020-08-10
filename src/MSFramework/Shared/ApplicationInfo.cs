using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MSFramework.DependencyInjection;

namespace MSFramework.Shared
{
	public class ApplicationInfo : IScopeDependency
	{
		private readonly IHostEnvironment _hostEnvironment;
		private readonly IConfiguration _configuration;

		public string Name
		{
			get
			{
				var applicationName = _configuration["ApplicationName"];
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