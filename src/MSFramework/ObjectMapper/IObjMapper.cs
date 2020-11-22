namespace MicroserviceFramework.ObjectMapper
{
	public interface IObjMapper
	{
		TDestination Map<TDestination>(object source);
		
		TDestination Map<TSource, TDestination>(TSource source);
		
		TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
	}
}