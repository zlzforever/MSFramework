namespace MSFramework.Mapper
{
	public interface IMapper
	{
		TDestination Map<TDestination>(object source);
		
		TDestination Map<TSource, TDestination>(TSource source);
		
		TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
	}
}