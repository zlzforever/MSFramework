using MicroserviceFramework.Domain;
using IAutoMapper = AutoMapper.IMapper;

namespace MicroserviceFramework.AutoMapper;

public class AutoMapperObjectAssembler(IAutoMapper mapper) : IObjectAssembler
{
    public TDestination To<TDestination>(object source)
    {
        return mapper.Map<TDestination>(source);
    }

    public TDestination To<TSource, TDestination>(TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }

    public TDestination To<TSource, TDestination>(TSource source, TDestination destination)
    {
        return mapper.Map(source, destination);
    }
}
