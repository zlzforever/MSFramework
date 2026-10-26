using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Template.Application.Products.Events;
using Template.Domain.Aggregates;

namespace Template.Infrastructure.Events;

/// <summary>
/// 提交后通过 Dapr 尽力发布集成通知，依赖当前工作单元及 DaprClient。
/// 此样例不保证投递可靠性；不可丢失的通知应替换为与业务同事务的 Outbox。
/// </summary>
/// <param name="unitOfWork">当前请求的主工作单元。</param>
/// <param name="client">由容器管理生命周期的 Dapr 客户端。</param>
/// <param name="logger">发布失败日志。</param>
public class ProductCreatedDomainEventHandler(IUnitOfWork unitOfWork, DaprClient client,
    ILogger<ProductCreatedDomainEventHandler> logger) : IDomainEventHandler<ProductCreatedEvent>
{
    /// <summary>订阅一次性提交回调，不嵌套保存业务上下文。</summary>
    /// <param name="event">已创建商品事件，不允许为空。</param>
    /// <param name="cancellationToken">当前保存操作取消令牌。</param>
    /// <returns>订阅完成的任务，而非消息投递完成。</returns>
    /// <exception cref="ArgumentNullException">事件为空。</exception>
    /// <exception cref="OperationCanceledException">保存操作已取消。</exception>
    public Task HandleAsync(ProductCreatedEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();
        Func<CancellationToken, ValueTask>? callback = null;
        callback = async token =>
        {
            // 提交后只尝试一次；发布失败不翻转已成功的业务提交，必须通过日志对账补发。
            unitOfWork.SavedChanges -= callback;
            try
            {
                await client.PublishEventAsync("pubsub", "ProductCreated",
                    new ProductCreatedIntegrationEvent(@event.ProductId), cancellationToken: token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "商品已提交但通知发布失败，需对账补发 {ProductId}", @event.ProductId);
            }
        };
        unitOfWork.SavedChanges += callback;
        return Task.CompletedTask;
    }

    /// <summary>无自有资源，DaprClient 和工作单元由容器释放。</summary>
    public void Dispose() { }
}
