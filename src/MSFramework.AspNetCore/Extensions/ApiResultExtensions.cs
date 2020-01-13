using MSFramework.Data;

namespace MSFramework.AspNetCore.Extensions
{
	public static class ApiResultExtensions
	{
		public static PagedApiResult<TEntity> ToPagedApiResult<TEntity>(this PagedQueryResult<TEntity> result)
		{
			result.NotNull(nameof(result));
			return new PagedApiResult<TEntity>(result);
		}
	}
}