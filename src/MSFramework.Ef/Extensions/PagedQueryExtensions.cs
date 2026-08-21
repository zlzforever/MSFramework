using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MicroserviceFramework.Linq.Expression;

/// <summary>
/// EF Core 分页查询扩展方法。
/// </summary>
/// <remarks>
/// 此 API 随 EF Core 依赖位于 <c>MSFramework.Ef</c> 包中，公共命名空间仍为
/// <c>MicroserviceFramework.Linq.Expression</c>。Core-only 消费者需要显式引用
/// <c>MSFramework.Ef</c> 后才能继续使用该扩展；核心包本身不再携带 EF Core 依赖。
/// </remarks>
public static class PagedQueryExtensions
{
    /// <param name="queryable">EF Core 或内存查询对象</param>
    /// <typeparam name="TEntity">实体类型</typeparam>
    extension<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        /// <summary>
        /// 异步分页查询。EF Core 查询走数据库端 CountAsync/ToListAsync，
        /// 内存查询（LINQ to Objects）回退为同步执行。
        /// </summary>
        /// <param name="page">页码，小于 1 时按 1 处理</param>
        /// <param name="limit">每页数量，小于 1 时按 10 处理</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页结果</returns>
        public async Task<PaginationResult<TEntity>> PagedQueryAsync(int page, int limit,
            CancellationToken cancellationToken = default)
        {
            page = page < 1 ? 1 : page;
            limit = limit < 1 ? 10 : limit;

            var total = await CountAsync(queryable, cancellationToken);
            var data = TryGetOffset(page, limit, total, out var offset)
                ? await ToListAsync(queryable.Skip(offset).Take(limit), cancellationToken)
                : [];

            return new PaginationResult<TEntity>(page, limit, total, data);
        }

        /// <summary>
        /// 异步分页查询并通过映射函数转换为 DTO。EF Core 查询走数据库端异步执行，
        /// 内存查询（LINQ to Objects）回退为同步执行。
        /// </summary>
        /// <param name="page">页码，小于 1 时按 1 处理</param>
        /// <param name="limit">每页数量，小于 1 时按 10 处理</param>
        /// <param name="mapper">实体到 DTO 的映射函数，不允许为 null</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <typeparam name="TDto">DTO 类型</typeparam>
        /// <returns>分页结果</returns>
        /// <exception cref="ArgumentNullException">mapper 为 null 时抛出</exception>
        public async Task<PaginationResult<TDto>> PagedQueryAsync<TDto>(int page, int limit,
            Func<TEntity, TDto> mapper, CancellationToken cancellationToken = default)
        {
            Check.NotNull(mapper, nameof(mapper));

            page = page < 1 ? 1 : page;
            limit = limit < 1 ? 10 : limit;

            var total = await CountAsync(queryable, cancellationToken);
            List<TDto> data;
            if (!TryGetOffset(page, limit, total, out var offset))
            {
                data = [];
            }
            else
            {
                var paged = queryable.Skip(offset).Take(limit);
                data = paged.Provider is IAsyncQueryProvider
                    ? (await paged.ToListAsync(cancellationToken)).Select(mapper).ToList()
                    : paged.AsEnumerable().Select(mapper).ToList();
            }

            return new PaginationResult<TDto>(page, limit, total, data);
        }
    }

    /// <summary>
    /// 计算可传给 LINQ Skip 的偏移量。偏移量超过 int 范围时，
    /// 该页不可能包含查询总数内的数据，直接返回空页。
    /// </summary>
    private static bool TryGetOffset(int page, int limit, int total, out int offset)
    {
        var longOffset = ((long)page - 1) * limit;
        if (longOffset > int.MaxValue)
        {
            offset = 0;
            return false;
        }

        if (longOffset >= total)
        {
            offset = 0;
            return false;
        }

        offset = (int)longOffset;
        return true;
    }

    /// <summary>
    /// 按查询提供器类型选择异步或同步计数。
    /// </summary>
    private static async Task<int> CountAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        return source.Provider is IAsyncQueryProvider
            ? await source.CountAsync(cancellationToken)
            : source.Count();
    }

    /// <summary>
    /// 按查询提供器类型选择异步或同步转列表。
    /// </summary>
    private static async Task<List<T>> ToListAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        return source.Provider is IAsyncQueryProvider
            ? await source.ToListAsync(cancellationToken)
            : source.ToList();
    }
}
