using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Common;

public interface IPagedResult : IEnumerable
{
    /// <summary>
    /// 总计
    /// </summary>
    int Total { get; }

    /// <summary>
    /// 当前页数 
    /// </summary>
    int Page { get; }

    /// <summary>
    /// 每页数据量 
    /// </summary>
    int Limit { get; }
}

public record PagedResult<TEntity>(int Page, int Limit, int Total, IEnumerable<TEntity> Data) : IEnumerable<TEntity>,
    IPagedResult
{
    /// <summary>
    /// 数据列表
    /// </summary>
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

    public IEnumerator<TEntity> GetEnumerator()
    {
        return Data == null ? Enumerable.Empty<TEntity>().GetEnumerator() : Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
