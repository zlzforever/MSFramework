using MicroserviceFramework.Mediator;
using Template.Domain.Repositories;

namespace Template.Application.Products.Commands;

/// <summary>通过商品仓储标记删除，由工作单元应用软删除审计；不存在时保持幂等。</summary>
/// <param name="productRepository">当前作用域商品仓储。</param>
public class DeleteProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<DeleteProductCommand>
{
    /// <summary>将商品标记为待删除，不在处理器内部提交。</summary>
    /// <param name="request">删除命令，不允许为空。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    /// <returns>仓储标记完成时完成的任务。</returns>
    /// <exception cref="ArgumentNullException">命令为空。</exception>
    /// <exception cref="OperationCanceledException">请求已取消。</exception>
    public async Task HandleAsync(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        await productRepository.DeleteAsync(request.Id);
    }
}
