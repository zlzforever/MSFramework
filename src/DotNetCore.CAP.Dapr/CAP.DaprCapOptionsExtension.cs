using System;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.CAP.Dapr;

internal class DaprCapOptionsExtension : ICapOptionsExtension
{
    private readonly Action<DaprOptions> _configure;

    public DaprCapOptionsExtension(Action<DaprOptions> configure)
    {
        _configure = configure;
    }

    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapMessageQueueMakerService("Dapr"));

        services.Configure(_configure);

        services.AddSingleton<ITransport, DaprTransport>();
        services.AddSingleton<IConsumerClientFactory, DaprConsumerClientFactory>();
    }
}
