using MSFramework.Data;

namespace MSFramework.AspNetCore
{
	public class PagedApiResult<TEntity> : ApiResult
	{
		public PagedApiResult(PagedQueryResult<TEntity> result) : base(new
		{
			success = true,
			// LayUI code = 0 为正常值
			code = 0,
			msg = string.Empty,
			count = result.Total,
			limit = result.Limit,
			page = result.Page,
			data = result.Entities
		})
		{
		}
	}
}