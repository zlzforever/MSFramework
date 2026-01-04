using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.Linq.Expression;

/// <summary>
///
/// </summary>
public static class PagedQueryExtensions
{
    /// <param name="queryable"></param>
    /// <typeparam name="TEntity"></typeparam>
    extension<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Task<PaginationResult<TEntity>> PagedQueryAsync(int page, int limit)
        {
            page = page < 1 ? 1 : page;
            limit = limit < 1 ? 10 : limit;

            var total = queryable.Count();
            var data = total == 0
                ? []
                : queryable.Skip((page - 1) * limit).Take(limit).ToList();

            return Task.FromResult(new PaginationResult<TEntity>(page, limit, total, data));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="mapper"></param>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        public Task<PaginationResult<TDto>> PagedQueryAsync<TDto>(int page, int limit, Func<TEntity, TDto> mapper)
        {
            Check.NotNull(mapper, nameof(mapper));

            page = page < 1 ? 1 : page;
            limit = limit < 1 ? 10 : limit;

            var total = queryable.Count();
            var data = total == 0
                ? []
                : queryable.Skip((page - 1) * limit).Take(limit).AsEnumerable().Select(mapper).ToList();

            return Task.FromResult(new PaginationResult<TDto>(page, limit, total, data));
        }
    }
}
