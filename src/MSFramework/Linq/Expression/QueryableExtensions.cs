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
/// 分页查询扩展方法
/// </summary>
public static class PagedQueryExtensions
{
    /// <param name="queryable"></param>
    /// <typeparam name="TEntity"></typeparam>
    extension<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        /// <summary>
        /// 异步分页查询。EF Core 查询走数据库端 CountAsync/ToListAsync（异步非阻塞），
        /// 内存查询（LINQ to Objects）回退为同步执行
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
            var data = total == 0
                ? []
                : await ToListAsync(queryable.Skip((page - 1) * limit).Take(limit), cancellationToken);

            return new PaginationResult<TEntity>(page, limit, total, data);
        }

        /// <summary>
        /// 异步分页查询并通过映射函数转换为 DTO。EF Core 查询走数据库端异步执行，
        /// 内存查询（LINQ to Objects）回退为同步执行
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
            if (total == 0)
            {
                data = [];
            }
            else
            {
                var paged = queryable.Skip((page - 1) * limit).Take(limit);
                data = paged.Provider is IAsyncQueryProvider
                    ? (await paged.ToListAsync(cancellationToken)).Select(mapper).ToList()
                    : paged.AsEnumerable().Select(mapper).ToList();
            }

            return new PaginationResult<TDto>(page, limit, total, data);
        }
    }

    /// <summary>
    /// 按查询提供器类型选择异步或同步计数
    /// </summary>
    /// <param name="source">查询对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>总数</returns>
    private static async Task<int> CountAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        return source.Provider is IAsyncQueryProvider
            ? await source.CountAsync(cancellationToken)
            : source.Count();
    }

    /// <summary>
    /// 按查询提供器类型选择异步或同步转列表
    /// </summary>
    /// <param name="source">查询对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据列表</returns>
    private static async Task<List<T>> ToListAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
    {
        return source.Provider is IAsyncQueryProvider
            ? await source.ToListAsync(cancellationToken)
            : source.ToList();
    }
}
