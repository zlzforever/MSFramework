using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Common;

// public interface IPagedResult : IEnumerable
// {
//     /// <summary>
//     /// 总计
//     /// </summary>
//     int Total { get; }
//
//     /// <summary>
//     /// 当前页数
//     /// </summary>
//     int Page { get; }
//
//     /// <summary>
//     /// 每页数据量
//     /// </summary>
//     int Limit { get; }
// }

// public class PagedResult : PagedResult<object>
// {
//     public PagedResult(int page, int limit, int total, IEnumerable<object> data) : base(page, limit, total, data)
//     {
//     }
// }

public class PagedResult<TEntity>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public IEnumerable<TEntity> Data { get; }

    /// <summary>
    /// 总计
    /// </summary>
    public int Total { get; }

    /// <summary>
    /// 当前页数
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// 每页数据量
    /// </summary>
    public int Limit { get; }

    public PagedResult(int page, int limit, int total, IEnumerable<TEntity> data)
    {
        Page = page;
        Limit = limit;
        Total = total;
        Data = data ?? Enumerable.Empty<TEntity>();
    }
}
