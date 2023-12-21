using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef;

public class EntityFrameworkBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
