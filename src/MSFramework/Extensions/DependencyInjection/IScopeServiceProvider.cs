namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
/// 作用域级服务提供程序，提供从当前 Scope 解析服务的能力。
/// </summary>
public interface IScopeServiceProvider
{
    /// <summary>
    /// 从当前作用域获取指定类型的服务实例。
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    T GetService<T>();
}
