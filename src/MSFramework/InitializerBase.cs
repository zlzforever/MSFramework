using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public abstract class InitializerBase : IInitializer
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public abstract void Start();

    /// <summary>
    /// 同步启动
    /// </summary>
    public bool Synchronized { get; protected init; }

    /// <summary>
    ///
    /// </summary>
    public int Order { get; protected init; }
}
