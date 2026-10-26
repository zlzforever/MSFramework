using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Common;

/// <summary>
/// 分页结果包装类
/// </summary>
/// <typeparam name="TEntity">分页数据项类型</typeparam>
public class PaginationResult<TEntity>(int page, int limit, int total, IEnumerable<TEntity> data)
{
    /// <summary>
    /// 数据列表。构造时立即物化为 <see cref="IReadOnlyList{TEntity}"/>：
    /// 若传入 EF 延迟查询（IQueryable），物化发生在构造时而非序列化阶段，
    /// 避免 DbContext 已释放后再枚举抛出 <c>ObjectDisposedException</c>，
    /// 同时保证重复读取 Data 不会重复执行数据库查询。
    /// </summary>
    public IReadOnlyList<TEntity> Data { get; } = data is null ? [] : data as IReadOnlyList<TEntity> ?? data.ToList();

    /// <summary>
    /// 数据总量
    /// </summary>
    public int Total { get; } = total;

    /// <summary>
    /// 当前页
    /// </summary>
    public int Page { get; } = page;

    /// <summary>
    /// 分页数据量
    /// </summary>
    public int Limit { get; } = limit;
}
