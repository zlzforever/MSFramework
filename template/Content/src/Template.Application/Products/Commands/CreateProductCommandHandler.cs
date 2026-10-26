using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Template.Domain.Aggregates;
using Template.Domain.Repositories;

namespace Template.Application.Products.Commands;

/// <summary>通过商品仓储创建聚合，提交及审计由框架工作单元过滤器统一完成。</summary>
/// <param name="productRepository">当前作用域的商品仓储。</param>
public class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ObjectId>
{
    /// <summary>校验领域不变量并将新商品加入仓储，不在处理器内部提前提交。</summary>
    /// <param name="request">创建输入，不允许为空。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    /// <returns>新商品标识。</returns>
    /// <exception cref="ArgumentNullException">输入为空。</exception>
    /// <exception cref="MicroserviceFramework.MicroserviceFrameworkFriendlyException">名称或价格无效。</exception>
    /// <exception cref="OperationCanceledException">请求已取消。</exception>
    public async Task<ObjectId> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var product = Product.Create(request.Name, request.Price, request.ProductType);
        await productRepository.AddAsync(product);
        return product.Id;
    }
}
