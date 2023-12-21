namespace MicroserviceFramework.Domain;

/// <summary>
/// 对象装载器
/// 不建议使用 mapper 工具，比如有一个业务对象 A 转换为 B
/// A 中增加了字段，但 B 中忘记添加
/// A 中字段 P1 改名为 P2，B 中没有同步修改
/// </summary>
public interface IObjectAssembler
{
    TDestination To<TDestination>(object source);

    TDestination To<TSource, TDestination>(TSource source);

    TDestination To<TSource, TDestination>(TSource source, TDestination destination);
}
