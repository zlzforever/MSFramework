namespace MicroserviceFramework.Domain
{
    public interface IObjectAssembler
    {
        TDestination To<TDestination>(object source);

        TDestination To<TSource, TDestination>(TSource source);

        TDestination To<TSource, TDestination>(TSource source, TDestination destination);
    }
}