# LocalEvent（本地事件）

进程内、**尽力而为（best-effort）**的事件通道，用于不影响主业务的旁路处理（发送邮件/短信、统计上报等）。

## 可靠性语义（使用前必读）

- **无重试、无死信**：`HandleAsync` 抛出的异常由后台服务捕获并仅记录日志，事件即被丢弃，不会重投。
  需要重试/补偿的业务必须在 Handler 内自行实现（try-catch、退避、状态表等），并保证 Handler 幂等；
- **停机不保证排空**：Host 停止时管道中未消费的事件直接丢失，发布成功 ≠ 一定被处理；
- **发布端背压**：管道容量 2000（有界），写满时最多等待 `PublishTimeout`（默认 30 秒），超时抛出 `TimeoutException`；
- Handler 在独立 Task + 独立 Scope 中执行，可注入 Scoped 服务（含 DbContext），
  但其工作单元与发布方相互独立——发布方需先提交自身工作单元，避免两边数据不一致。

## 更高可靠性要求

不可丢失、需要重试/死信/跨服务分发的场景，请使用**集成事件（消息队列）**：
CAP 本地消息表（Outbox，与业务变更同事务落库、提交后投递）、Dapr Pub/Sub、RabbitMQ 等。

## 与领域事件的关系

聚合根领域事件（`AddDomainEvent`）与本业务**同事务**派发（见 `DbContextBase`），
保证本地 Handler 的数据变更与业务原子提交；本地事件通道不做这个保证，不要用本地事件替代需要事务一致性的处理。
# 发布背压超时

队列满载时最多等待 `LocalEventOptions.PublishTimeout`（默认 30 秒），超时抛出 `TimeoutException`，
调用方主动取消仍抛出 `OperationCanceledException`。超时只限制入队等待，不限制 Handler 执行时长，
也不改变进程内、无重试、尽力而为语义。Handler 再次发布事件时也受同一上限约束，避免永久等待唯一消费者。

```csharp
builder.UseLocalEventPublisher(options =>
{
    options.PublishTimeout = TimeSpan.FromSeconds(3);
});
```

通过 `IEventPublisher.PublishAsync(event, timeout, cancellationToken)` 可覆盖单次发布的等待上限，
不修改全局配置。`timeout` 为 `null` 时使用全局配置（默认 30 秒）；非空时必须大于零且不超过 4294967294 毫秒，不支持无限等待；
不传 `timeout` 的原有调用继续使用全局配置。
