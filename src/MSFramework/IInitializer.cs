using MicroserviceFramework.Extensions.DependencyInjection;

namespace MicroserviceFramework;

/// <summary>
/// 框架初始化器接口，在应用启动时执行初始化逻辑
/// </summary>
internal interface IInitializer : ISingletonDependency
{
    /// <summary>
    /// 执行初始化操作
    /// </summary>
    void Start();

    /// <summary>
    /// 初始化执行顺序，值越小越先执行
    /// </summary>
    public int Order { get; }
}
