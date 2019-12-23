using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.Domain;

namespace MSFramework.Ef
{
	public static class PagedQueryExtensions
	{
		public static async Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey>(
			this EfRepository<TEntity> repository,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null)
			where TEntity : class, IAggregateRoot, IEntity
		{
			return await repository.Entities.PagedQueryAsync<TEntity, TOrderKey, object>(page, limit, where, orderBy);
		}
	}
}