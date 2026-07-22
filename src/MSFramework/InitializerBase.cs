namespace MicroserviceFramework;

/// <summary>
/// 初始化器基类，继承此类并实现 <see cref="Start"/> 方法完成自定义初始化
/// </summary>
public abstract class InitializerBase : IInitializer
{
    /// <summary>
    /// 执行初始化操作（由子类实现具体逻辑）
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// 初始化执行顺序，值越小越先执行
    /// </summary>
    public int Order { get; protected set; }
}
