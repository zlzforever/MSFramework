using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework.Common;

/// <summary>
/// 应用程序信息，包含应用名称和版本。
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 从主机环境和配置中初始化应用信息。
    /// </summary>
    /// <param name="hostEnvironment">主机环境</param>
    /// <param name="configuration">应用配置</param>
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
