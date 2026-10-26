using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;

namespace Template.Application.Products.Queries;

/// <summary>通过应用层读取契约查询商品分页，不持有数据库上下文。</summary>
/// <param name="queries">由基础设施注入的商品读取实现。</param>
public class PagedProductQueryHandler(IProductQueries queries)
    : IRequestHandler<PagedProductQuery, PaginationResult<ProductOut>>
{
    /// <summary>转发分页请求至数据库投影实现。</summary>
    /// <param name="request">分页条件，不允许为空。</param>
    /// <param name="cancellationToken">查询取消令牌。</param>
    /// <returns>当前页及总数。</returns>
    /// <exception cref="ArgumentNullException">请求为空。</exception>
    /// <exception cref="OperationCanceledException">查询已取消。</exception>
    public Task<PaginationResult<ProductOut>> HandleAsync(PagedProductQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return queries.PagedAsync(request, cancellationToken);
    }
}
