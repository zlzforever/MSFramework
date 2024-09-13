using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
/// <param name="services"></param>
public class MicroserviceFrameworkBuilder(IServiceCollection services)
{
    /// <summary>
    ///
    /// </summary>
    public IServiceCollection Services { get; } = services;
}
