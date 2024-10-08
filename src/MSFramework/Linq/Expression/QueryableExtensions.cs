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
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="page"></param>
    /// <param name="limit"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static Task<PaginationResult<TEntity>> PagedQueryAsync<TEntity>(
        this IQueryable<TEntity> queryable,
        int page, int limit)
        where TEntity : class
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
    /// <param name="queryable"></param>
    /// <param name="page"></param>
    /// <param name="limit"></param>
    /// <param name="mapper"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static Task<PaginationResult<TDto>> PagedQueryAsync<TEntity, TDto>(
        this IQueryable<TEntity> queryable,
        int page, int limit, Func<TEntity, TDto> mapper)
        where TEntity : class
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
