using MicroserviceFramework.Domain;
using IAutoMapper = AutoMapper.IMapper;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///     基于 AutoMapper 的对象映射器实现，用于 DTO 与实体间的转换
/// </summary>
/// <param name="mapper">AutoMapper 映射器</param>
public class AutoMapperObjectAssembler(IAutoMapper mapper) : IObjectAssembler
{
    /// <summary>
    ///     将源对象映射到指定目标类型
    /// </summary>
    /// <param name="source">源对象</param>
    /// <typeparam name="TDestination">目标类型</typeparam>
    /// <returns>映射后的目标对象</returns>
    public TDestination To<TDestination>(object source)
    {
        return mapper.Map<TDestination>(source);
    }

    /// <summary>
    ///     将源对象映射到指定目标类型（强类型版本）
    /// </summary>
    /// <param name="source">源对象</param>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TDestination">目标类型</typeparam>
    /// <returns>映射后的目标对象</returns>
    public TDestination To<TSource, TDestination>(TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }

    /// <summary>
    ///     将源对象的值映射到已有的目标对象实例上
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="destination">已有的目标对象</param>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TDestination">目标类型</typeparam>
    /// <returns>映射后的目标对象</returns>
    public TDestination To<TSource, TDestination>(TSource source, TDestination destination)
    {
        return mapper.Map(source, destination);
    }
}
