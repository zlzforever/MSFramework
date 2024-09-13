namespace MicroserviceFramework.Domain;

/// <summary>
/// 对象装载器
/// 不建议使用 mapper 工具，比如有一个业务对象 A 转换为 B
/// A 中增加了字段，但 B 中忘记添加
/// A 中字段 P1 改名为 P2，B 中没有同步修改
/// </summary>
public interface IObjectAssembler
{
    /// <summary>
    /// 转换对象
    /// </summary>
    /// <param name="source">源对象</param>
    /// <typeparam name="TDestination">目标对象类型</typeparam>
    /// <returns></returns>
    TDestination To<TDestination>(object source);

    /// <summary>
    /// 转换对象
    /// </summary>
    /// <param name="source">源对象</param>
    /// <typeparam name="TSource">源对象类型</typeparam>
    /// <typeparam name="TDestination">目标对象类型</typeparam>
    /// <returns></returns>
    TDestination To<TSource, TDestination>(TSource source);

    /// <summary>
    /// 转换对象
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="destination">目标对象</param>
    /// <typeparam name="TSource">源对象类型</typeparam>
    /// <typeparam name="TDestination">目标对象类型</typeparam>
    /// <returns></returns>
    TDestination To<TSource, TDestination>(TSource source, TDestination destination);
}
