using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Linq.Expression;

public static class PagedQueryExtensions
{
    public static Task<PaginationResult<TEntity>> PagedQueryAsync<TEntity>(
        this IQueryable<TEntity> queryable,
        int page, int limit)
        where TEntity : class
    {
        page = page < 1 ? 1 : page;
        limit = limit < 1 ? 10 : limit;
        limit = limit > 100 ? 100 : limit;

        var total = queryable.Count();
        var data = total == 0
            ? []
            : queryable.Skip((page - 1) * limit).Take(limit).ToList();

        return Task.FromResult(new PaginationResult<TEntity>(page, limit, total, data));
    }
}
