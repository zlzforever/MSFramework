using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

public interface IInitializerBase : ISingletonDependency
{
    Task StartAsync(CancellationToken cancellationToken);
    int Order { get; }
}
