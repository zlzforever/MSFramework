using MicroserviceFramework.Common;
using MicroserviceFramework.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace Template.Application.Products.Queries;

/// <summary>商品读取契约，由基础设施实现投影与数据库分页，应用层不依赖 EF 或具体上下文。</summary>
public interface IProductQueries : IScopeDependency
{
    /// <summary>读取未删除商品。</summary>
    /// <param name="id">商品标识。</param>
    /// <param name="cancellationToken">查询取消令牌。</param>
    /// <returns>商品输出，不存在时为 null。</returns>
    /// <exception cref="OperationCanceledException">查询已取消。</exception>
    Task<ProductOut?> GetAsync(ObjectId id, CancellationToken cancellationToken);

    /// <summary>按稳定排序读取一页商品。</summary>
    /// <param name="query">分页及关键词条件，不允许为空。</param>
    /// <param name="cancellationToken">查询取消令牌。</param>
    /// <returns>含总数与当前页输出的分页结果。</returns>
    /// <exception cref="ArgumentNullException">条件为空。</exception>
    /// <exception cref="OperationCanceledException">查询已取消。</exception>
    Task<PaginationResult<ProductOut>> PagedAsync(PagedProductQuery query, CancellationToken cancellationToken);
}
