using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;

namespace Template.Application.Products.Events;

/// <summary>集成事件接收样例，只记录标识；实际业务应在此增加幂等处理。</summary>
/// <param name="logger">当前处理器日志记录器，不记录令牌或完整载荷。</param>
public class ProductCreatedIntegrationEventHandler(ILogger<ProductCreatedIntegrationEventHandler> logger)
    : IRequestHandler<ProductCreatedIntegrationEvent>
{
    /// <summary>记录收到的商品通知。</summary>
    /// <param name="request">通知，不允许为空。</param>
    /// <param name="cancellationToken">Dapr 回调请求取消令牌。</param>
    /// <returns>完成的任务。</returns>
    /// <exception cref="ArgumentNullException">通知为空。</exception>
    /// <exception cref="OperationCanceledException">回调已取消。</exception>
    public Task HandleAsync(ProductCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("收到商品创建通知 {ProductId}", request.ProductId);
        return Task.CompletedTask;
    }
}
