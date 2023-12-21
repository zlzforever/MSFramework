using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework;

public class MicroserviceFrameworkBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
