using System.Collections.Generic;
using MSFramework.Data;

namespace MSFramework.Extensions
{
	public static class MapperExtensions
	{
		public static PagedQueryResult<DTO> ToPagedQueryResultDTO<DTO>(this IMapper mapper, IPagedQueryResult result)
		{
			mapper.NotNull(nameof(mapper));
			result.NotNull(nameof(result));

			return new PagedQueryResult<DTO>
			{
				Page = result.Page,
				Limit = result.Limit,
				Total = result.Total,
				Data = mapper.Map<List<DTO>>(result.GetEntities())
			};
		}
	}
}