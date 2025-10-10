using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
internal interface IInitializerBase : ISingletonDependency
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 同步启动
    /// </summary>
    bool Synchronized { get; }

    /// <summary>
    ///
    /// </summary>
    public int Order { get; }
}
