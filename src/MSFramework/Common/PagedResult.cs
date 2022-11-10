using System.Collections.Generic;

namespace MicroserviceFramework.Common;

public record PagedResult<TEntity>(int Page, int Limit, int Total, IEnumerable<TEntity> Data)
{
    public IEnumerable<TEntity> Data { get; } = Data;

    /// <summary>
    /// 总计
    /// </summary>
    public int Total { get; } = Total;

    /// <summary>
    /// 当前页数 
    /// </summary>
    public int Page { get; } = Page;

    /// <summary>
    /// 每页数据量 
    /// </summary>
    public int Limit { get; } = Limit;
}
