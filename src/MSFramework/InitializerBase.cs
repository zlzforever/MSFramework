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
    /// 初始化执行顺序，值越大越先执行。
    /// 框架的 EF 迁移初始化器使用 <see cref="int.MaxValue"/>，确保数据库迁移先于
    /// 依赖表结构的业务初始化器（如数据播种）执行；相同 Order 按注册顺序稳定执行。
    /// </summary>
    public int Order { get; protected set; }
}
