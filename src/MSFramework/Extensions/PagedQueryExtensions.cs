using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.Http;

namespace MSFramework.Extensions
{
	public static class PagedQueryExtensions
	{
		public static LayUIApiPagedQueryResult<TEntity> ToLayUIApiPagedQueryResult<TEntity>(
			PagedQueryResult<TEntity> result)
		{
			return new LayUIApiPagedQueryResult<TEntity>(result.Page, result.Limit, result.Count, result.Data);
		}

		public static LayUIApiPagedQueryResult ToLayUIApiPagedQueryResult(
			PagedQueryResult<object> result)
		{
			return new LayUIApiPagedQueryResult(result.Page, result.Limit, result.Count, result.Data);
		}

		public static async Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null)
			where TEntity : class
		{
			return await queryable.PagedQueryAsync<TEntity, TOrderKey, object>(page, limit, where, orderBy);
		}

		public static async Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null,
			OrderCondition<TEntity, TThenOrderKey> thenBy = null)
			where TEntity : class
		{
			return await queryable.PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey, object>(page, limit, where,
				orderBy, thenBy);
		}

		public static Task<PagedQueryResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey1,
			TThenOrderKey2>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null,
			OrderCondition<TEntity, TThenOrderKey1> thenBy1 = null,
			OrderCondition<TEntity, TThenOrderKey2> thenBy2 = null)
			where TEntity : class
		{
			var result = new PagedQueryResult<TEntity>();
			page = page < 1 ? 1 : page;
			limit = limit < 10 ? 10 : limit;
			var entities = where == null ? queryable : queryable.Where(where);
			if (orderBy != null)
			{
				entities = !orderBy.Desc
					? entities.OrderBy(orderBy.Expression)
					: entities.OrderByDescending(orderBy.Expression);
			}

			if (thenBy1 != null)
			{
				if (orderBy == null)
				{
					throw new ArgumentException("Order by should not be null when use then by");
				}

				entities = !thenBy1.Desc
					? ((IOrderedQueryable<TEntity>) entities).ThenBy(thenBy1.Expression)
					: ((IOrderedQueryable<TEntity>) entities).OrderByDescending(thenBy1.Expression);
			}

			if (thenBy2 != null)
			{
				if (orderBy == null)
				{
					throw new ArgumentException("Order by should not be null when use then by");
				}

				entities = !thenBy2.Desc
					? ((IOrderedQueryable<TEntity>) entities).ThenBy(thenBy2.Expression)
					: ((IOrderedQueryable<TEntity>) entities).OrderByDescending(thenBy2.Expression);
			}

			result.Count = entities.Count();
			result.Page = page;
			result.Limit = limit;

			entities = entities.Skip((result.Page - 1) * result.Limit).Take(result.Limit);

			result.Data = result.Count == 0 ? new List<TEntity>() : entities.ToList();
			return Task.FromResult(result);
		}
	}
}