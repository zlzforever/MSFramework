using System;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.CAP.Dapr;

internal class DaprCapOptionsExtension(Action<DaprOptions> configure) : ICapOptionsExtension
{
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapMessageQueueMakerService("Dapr"));

        services.Configure(configure);

        services.AddSingleton<ITransport, DaprTransport>();
        services.AddSingleton<IConsumerClientFactory, DaprConsumerClientFactory>();
    }
}
