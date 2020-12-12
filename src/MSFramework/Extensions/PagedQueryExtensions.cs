using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Extensions
{
	public static class PagedQueryExtensions
	{
		public static Task<PagedResult<TEntity>> PagedQueryAsync<TEntity>(
			this IQueryable<TEntity> queryable,
			int page, int limit)
			where TEntity : class
		{
			page = page < 1 ? 1 : page;
			limit = limit < 1 ? 10 : limit;
			var total = queryable.Count();
			var data = total == 0 ? new List<TEntity>() : queryable.Skip((page - 1) * limit).Take(limit).ToList();
			return Task.FromResult(new PagedResult<TEntity>(page, limit, total, data));
		}
	}
}