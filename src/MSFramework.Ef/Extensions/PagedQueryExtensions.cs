using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.Domain.AggregateRoot;
using MSFramework.Domain.Entity;
using MSFramework.Extensions;

namespace MSFramework.Ef.Extensions
{
	public static class PagedQueryExtensions
	{
		public static async Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey>(
			this EfRepository<TEntity, Guid> repository,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null)
			where TEntity : class, IAggregateRoot<Guid>, IEntity
		{
			return await repository.PagedQueryAsync<TEntity, Guid, TOrderKey>(page, limit, where, orderBy);
		}

		public static async Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TKey, TOrderKey>(
			this EfRepository<TEntity, TKey> repository,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null)
			where TEntity : class, IAggregateRoot<TKey>, IEntity where TKey : IEquatable<TKey>
		{
			return await repository.CurrentSet.PagedQueryAsync<TEntity, TOrderKey, object>(page, limit, where, orderBy);
		}
	}
}