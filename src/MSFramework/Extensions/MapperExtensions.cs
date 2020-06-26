using System.Collections.Generic;
using MSFramework.Data;
using MSFramework.Mapper;
using MSFramework.Common;

namespace MSFramework.Extensions
{
	public static class MapperExtensions
	{
		public static PagedResult<TDestination> MapPagedResult<TDestination>(this IObjectMapper mapper,
			PagedResult result)
		{
			mapper.NotNull(nameof(mapper));
			result.NotNull(nameof(result));

			return new PagedResult<TDestination>(result.Page, result.Limit, result.Count,
				mapper.Map<List<TDestination>>(result.Data));
		}
	}
}