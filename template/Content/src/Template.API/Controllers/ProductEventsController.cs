using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Products.Events;

namespace Template.API.Controllers;

/// <summary>Dapr 专用回调端点，与 JWT 业务控制器隔离，依赖官方 App API Token 认证策略。</summary>
/// <param name="mediator">框架事件请求分发器。</param>
[ApiController]
[Route("events/products")]
[Authorize(Policy = "Dapr")]
public class ProductEventsController(IMediator mediator) : ApiControllerBase
{
    /// <summary>接收 CloudEvent 商品通知，不套用普通业务接口的 JWT 策略。</summary>
    /// <param name="event">Dapr 解包后的通知，不允许为空。</param>
    /// <param name="cancellationToken">回调请求取消令牌。</param>
    /// <returns>应用处理完成时完成的任务。</returns>
    /// <exception cref="ArgumentNullException">通知为空。</exception>
    /// <exception cref="OperationCanceledException">请求取消。</exception>
    [HttpPost("created")]
    [Topic("pubsub", "ProductCreated")]
    public Task Created([FromBody] ProductCreatedIntegrationEvent @event, CancellationToken cancellationToken) =>
        mediator.SendAsync(@event, cancellationToken);
}
