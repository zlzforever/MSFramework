using Mapster;
using MSFramework.Data;

namespace MSFramework.Mapster
{
	public class Mapper : IMapper
	{
		public TDestination Map<TDestination>(object source)
		{
			source.NotNull(nameof(source));
			return source.Adapt<TDestination>();
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			source.NotNull(nameof(source));
			return source.Adapt<TDestination>();
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			return source.Adapt(destination);
		}
	}
}