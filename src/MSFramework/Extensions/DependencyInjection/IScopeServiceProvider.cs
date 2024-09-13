namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public interface IScopeServiceProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetService<T>();
}
