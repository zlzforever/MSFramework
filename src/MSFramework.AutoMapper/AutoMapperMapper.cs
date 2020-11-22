using MicroserviceFramework.ObjectMapper;
using IAutoMapper = AutoMapper.IMapper;

namespace MicroserviceFramework.AutoMapper
{
	public class AutoMapperMapper : IObjMapper
	{
		private readonly IAutoMapper _mapper;

		public AutoMapperMapper(IAutoMapper mapper)
		{
			_mapper = mapper;
		}

		public TDestination Map<TDestination>(object source)
		{
			return _mapper.Map<TDestination>(source);
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			return _mapper.Map<TSource, TDestination>(source);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			return _mapper.Map(source, destination);
		}
	}
}