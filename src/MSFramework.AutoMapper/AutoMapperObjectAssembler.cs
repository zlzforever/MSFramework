using MicroserviceFramework.Domain;
using IAutoMapper = AutoMapper.IMapper;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///
/// </summary>
/// <param name="mapper"></param>
public class AutoMapperObjectAssembler(IAutoMapper mapper) : IObjectAssembler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    public TDestination To<TDestination>(object source)
    {
        return mapper.Map<TDestination>(source);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    public TDestination To<TSource, TDestination>(TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    public TDestination To<TSource, TDestination>(TSource source, TDestination destination)
    {
        return mapper.Map(source, destination);
    }
}
