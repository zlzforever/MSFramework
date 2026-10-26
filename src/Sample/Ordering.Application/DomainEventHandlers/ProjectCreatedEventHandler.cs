using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class ProjectCreatedEventHandler(
    ILogger<ProjectCreatedEventHandler> logger,
    IUnitOfWork unitOfWork,
    DaprClient daprClient)
    : IDomainEventHandler<ProjectCreatedEvent>
{
    public Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // 领域事件 <-> 和当前上下文/SCOPE 同时成功、失败，同步
        // 集成事件 <-> 异步不管的
        // 领域事件的处理器，发送集成事件
        //
        // 一次性订阅：IUnitOfWork 为 scope 级对象，同一请求内可能触发多次 HandleAsync
        // （如批量创建多个聚合）。若每次都直接追加订阅而不解绑，SavedChanges 会在
        // 同一 scope 的每次提交时触发全部历史委托，把早前事件按旧载荷重复发布。
        // 委托在发布前先自我解绑，保证"一个事件只发布一次"。
        Func<CancellationToken, ValueTask> handler = null;
        handler = async c =>
        {
            // 先解绑再发布：即使发布失败也不会被下一次提交重复触发
            unitOfWork.SavedChanges -= handler;
            try
            {
                await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.ProjectCreatedEvent,
                    new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: c);
                logger.LogInformation("Publish ProjectCreatedEvent");
            }
            catch (Exception ex)
            {
                // 业务数据已提交，集成事件发布失败不应改变本次请求结果；
                // 按"集成事件异步不管"语义仅记录告警，由对账/Dapr pubsub 重试兜底
                logger.LogError(ex, "Publish ProjectCreatedEvent 失败，事件 {EventId}", @event.Id);
            }
        };
        unitOfWork.SavedChanges += handler;
        return Task.CompletedTask;
    }
}
