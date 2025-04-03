using System.Collections.Generic;

namespace MicroserviceFramework.Common;

/// <summary>
/// 分页结果包装类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class PaginationResult<TEntity>(int page, int limit, int total, List<TEntity> data)
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public ICollection<TEntity> Data { get; } = data ?? [];

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
