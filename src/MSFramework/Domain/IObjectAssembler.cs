namespace MicroserviceFramework.Domain;

/// <summary>
/// 对象装载器
/// </summary>
public interface IObjectAssembler
{
    TDestination To<TDestination>(object source);

    TDestination To<TSource, TDestination>(TSource source);

    TDestination To<TSource, TDestination>(TSource source, TDestination destination);
}
