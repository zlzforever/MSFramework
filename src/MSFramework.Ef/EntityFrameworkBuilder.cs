using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

public class EntityFrameworkBuilder
{
    public EntityFrameworkBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
