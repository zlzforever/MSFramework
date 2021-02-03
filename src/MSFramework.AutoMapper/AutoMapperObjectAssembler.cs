using MicroserviceFramework.Domain;
using IAutoMapper = AutoMapper.IMapper;

namespace MicroserviceFramework.AutoMapper
{
	public class AutoMapperObjectAssembler : IObjectAssembler
	{
		private readonly IAutoMapper _mapper;

		public AutoMapperObjectAssembler(IAutoMapper mapper)
		{
			_mapper = mapper;
		}

		public TDestination To<TDestination>(object source)
		{
			return _mapper.Map<TDestination>(source);
		}

		public TDestination To<TSource, TDestination>(TSource source)
		{
			return _mapper.Map<TSource, TDestination>(source);
		}

		public TDestination To<TSource, TDestination>(TSource source, TDestination destination)
		{
			return _mapper.Map(source, destination);
		}
	}
}