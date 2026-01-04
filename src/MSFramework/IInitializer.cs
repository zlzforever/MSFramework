using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
internal interface IInitializer : ISingletonDependency
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    void Start();

    /// <summary>
    ///
    /// </summary>
    public int Order { get; }
}
