using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public interface IInitializerBase : ISingletonDependency
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);
    /// <summary>
    ///
    /// </summary>
    int Order { get; }
}
