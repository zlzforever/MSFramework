using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderStartedDomainEventHandler(
    ILogger<OrderStartedDomainEventHandler> logger,
    IUnitOfWork unitOfWork)
    : IDomainEventHandler<OrderStartedDomainEvent>
{
    public Task HandleAsync(OrderStartedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        // 一次性订阅：IUnitOfWork 为 scope 级对象，同一请求内可能多次触发本 Handler，
        // 订阅委托必须在发布（提交）后自我解绑，避免同一 scope 多次 SaveChanges
        // 时历史委托重复执行（详见 ProjectCreatedEventHandler 的同款处理）
        Func<CancellationToken, ValueTask> handler = null;
        handler = async c =>
        {
            unitOfWork.SavedChanges -= handler;
            // await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.OrderCreatedEvent,
            //     new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
            logger.LogInformation("Publish OrderCreatedEvent");
            await Task.CompletedTask;
        };
        unitOfWork.SavedChanges += handler;
        return Task.CompletedTask;
    }
}
