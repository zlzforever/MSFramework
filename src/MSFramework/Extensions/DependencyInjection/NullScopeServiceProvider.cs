namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public class NullScopeServiceProvider : IScopeServiceProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetService<T>()
    {
        return default;
    }
}
