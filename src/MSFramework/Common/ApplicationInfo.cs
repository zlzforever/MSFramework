using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework.Common;

/// <summary>
///
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="hostEnvironment"></param>
    /// <param name="configuration"></param>
    public ApplicationInfo(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        var applicationName = configuration["ApiName"];
        applicationName = string.IsNullOrEmpty(applicationName)
            ? configuration["ApplicationName"]
            : applicationName;
        applicationName = string.IsNullOrEmpty(applicationName)
            ? hostEnvironment.ApplicationName
            : applicationName;
        applicationName = string.IsNullOrEmpty(applicationName)
            ? Assembly.GetEntryAssembly()?.GetName().Name
            : applicationName;
        Name = applicationName;
    }
}
