using MicroserviceFramework.Common;
using MicroserviceFramework.Linq.Expression;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Template.Application.Products;
using Template.Application.Products.Queries;
using Template.Domain.Aggregates;

namespace Template.Infrastructure.Queries;

/// <summary>使用当前作用域上下文执行只读投影、软删除过滤和数据库分页，不加载聚合导航。</summary>
/// <param name="context">主上下文，与请求作用域一致。</param>
public class ProductQueries(TemplateDbContext context) : IProductQueries
{
    /// <inheritdoc/>
    public Task<ProductOut?> GetAsync(ObjectId id, CancellationToken cancellationToken) =>
        Project(context.Set<Product>().Where(product => product.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<PaginationResult<ProductOut>> PagedAsync(PagedProductQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var products = context.Set<Product>().AsQueryable();
        var keyword = query.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            products = products.Where(product => product.Name.Contains(keyword));
        }

        return Project(products.OrderByDescending(product => product.CreationTime).ThenBy(product => product.Id))
            .PagedQueryAsync(query.Page, query.Limit, cancellationToken);
    }

    private static IQueryable<ProductOut> Project(IQueryable<Product> products) =>
        products.AsNoTracking().Select(product => new ProductOut(product.Id, product.Name, product.Price,
            product.ProductType, product.CreationTime));
}
