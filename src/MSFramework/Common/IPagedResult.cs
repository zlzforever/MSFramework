using System.Collections;

namespace MicroserviceFramework.Common;

public interface IPagedResult
{
    IEnumerable GetEnumerable();

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
