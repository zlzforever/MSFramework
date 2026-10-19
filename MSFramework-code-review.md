# MSFramework 第三轮深度代码复审

复审日期：2026-09-06。

## 复审范围与结论

本轮仅审查 `src/` 下目录名以 `MSFramework` 开头的项目：

- `MSFramework`
- `MSFramework.AspNetCore`
- `MSFramework.Ef`
- `MSFramework.Ef.MySql`
- `MSFramework.Ef.PostgreSql`
- `MSFramework.Ef.SqlServer`
- `MSFramework.Ef.Analyzers`
- `MSFramework.Analyzers`
- `MSFramework.AutoMapper`
- `MSFramework.Serialization.Newtonsoft`
- `MSFramework.Auditing.Loki`

排除 `src/Sample`、`tests`、`DotNetCore.CAP.Dapr`、发布脚本和解决方案元数据。报告针对当前工作树源码，不将用户已有修改回滚或视为本轮修复。

结论：仍有 4 项 P0，必须在发布前处理：Dapr Topic 鉴权 fail-open、领域事件清理/事务语义、多 DbContext 工作单元的部分提交，以及 MySQL 迁移全局移除外键。另有多项 P1，集中在本地事件可靠性、文件上传安全与原子性、AES 使用方式、审计数据泄露、仓储默认加载、分页契约、公开 API 兼容性和日志资源回收。

风险等级定义：`P0` 表示发布前必须修复；`P1` 表示近期必须修复；`P2` 表示应纳入后续版本或明确记录为契约限制。

验证记录：

- `tests/MSFramework.Tests` 可编译，结果为 223 passed、8 failed、1 skipped，共 232 项。8 个失败均来自 `ResponseWrapperFilterTests` 对 `DeclaredType` 仍断言为 `ApiResult.Type`；当前生产代码明确将包装后的 `ObjectResult.DeclaredType` 置为 `null`，响应值本身已正确包装，因此这是测试契约未同步，不应误判为包装逻辑完全失效。
- `tests/MSFramework.AspNetCore.Test`：55 passed、0 failed、0 skipped。
- `MSFramework.Ef.MySql`、`MSFramework.Ef.PostgreSql`、`MSFramework.Ef.SqlServer` 项目构建成功，0 warning、0 error。
- `ocr review --preview` 能识别工作树中的改动，但 `ocr llm test` 因无法解析 `api.deepseek.com` 失败，本报告结论来自本地源码、diff、构建和测试结果，未获得外部模型意见。
- 本轮未修改生产源码，仅更新本 Markdown 报告。

## 严重bug

### 1. Dapr Topic 鉴权是 fail-open

- 位置：[DaprSecurityMiddleware.cs:28-59](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs:28)、[HttpContextExtensions.cs:30-53](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/HttpContextExtensions.cs:30)
- 问题描述：注释声明使用 `DAPR_API_TOKEN`，但当前实现完全没有读取或校验 token。`Endpoint == null` 时直接放行；IP 判断无条件优先信任 `X-Forwarded-For`，公网调用者可以伪造 `127.0.0.1`。此外，逗号分隔的转发链没有解析，合法代理请求可能被错误拒绝。
- 风险等级：`P0`，`严重bug｜安全风险`。攻击者可伪造内部来源访问 Dapr 订阅端点，影响消息注入、越权调用和内部接口暴露。
- 复现条件：将中间件放在 `UseRouting` 前使 endpoint 未建立；或向公开端点发送 `X-Forwarded-For: 127.0.0.1`；或配置了 Dapr token 但发送错误/空 token。
- 根因分析：鉴权条件依赖客户端可控请求头，且路由元数据不存在时采用放行策略；注释中的认证契约没有落到代码和启动配置上。
- 参考修复代码：

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor,
    KnownProxies = { trustedProxyAddress }
});
app.UseRouting();
app.UseMiddleware<DaprSecurityMiddleware>();

var expected = configuration["DAPR_API_TOKEN"];
var supplied = context.Request.Headers["dapr-api-token"].ToString();
var tokenValid = !string.IsNullOrEmpty(expected)
    && CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));

if (isTopic && (endpoint == null || !tokenValid))
{
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    return;
}
```

- 方案取舍：token 应作为主认证；可信代理解析后的 IP 只能作为额外限制。只依赖 IP 实现简单但无法抵抗代理配置错误或 Header 伪造。若必须兼容无 token 的旧部署，应增加显式配置开关并默认拒绝，而不是隐式放行。

### 2. 领域事件在发布前清空，失败会丢失事件

- 位置：[DbContextBase.cs:180-205](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:180)、[DbContextBase.cs:411-434](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:411)
- 问题描述：`SaveChangesAsync` 先调用 `GetDomainEvents()`，而该方法在返回事件前执行 `ClearDomainEvents()`。随后只要 Handler 抛异常、取消令牌触发、`ApplyConcepts()` 返回 false，或数据库保存失败，事件都已从聚合中删除，无法重试。没有 `IMediator` 时事件不会派发，也没有配置错误信号。
- 风险等级：`P0`，`严重bug｜逻辑缺陷｜异常处理`。订单、库存等依赖事件的业务可能出现数据库状态与下游状态永久不一致。
- 复现条件：聚合添加领域事件后，Handler 抛异常；只添加事件但实体状态未变化；数据库约束或网络故障导致 `base.SaveChangesAsync` 失败；宿主未注册 `IMediator`。
- 根因分析：领域事件没有 Pending、Dispatching、Acknowledged 等状态，清理动作没有绑定到具体事件的成功确认点；当前代码把“取快照”和“确认删除”合并为一个操作。
- 参考修复代码（推荐 Outbox）：

```csharp
var pending = aggregates
    .SelectMany(x => x.DequeueDomainEvents())
    .ToArray();

foreach (var domainEvent in pending)
{
    AddOutboxMessage(domainEvent); // 与聚合变更处于同一数据库事务
}

await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
// Outbox 记录提交成功后由后台投递，失败可重试并进入死信。
```

- 方案取舍：Outbox 能把业务变更和事件记录放进同一事务，适合跨进程副作用，可靠性最高但需要表、投递器和幂等消费者。若暂时保留同步发布，至少应按事件实例 dequeue，成功后 ack，失败时 requeue；这仍无法覆盖进程崩溃和外部副作用，不能替代 Outbox。

### 3. 多 DbContext 工作单元不是原子提交

- 位置：[EfUnitOfWork.cs:29-34](/Users/lewis/github/MSFramework/src/MSFramework.Ef/EfUnitOfWork.cs:29)
- 问题描述：`GetAllDbContexts()` 返回的上下文被逐个调用 `SaveChangesAsync`，每个上下文使用自己的默认事务。第一个上下文成功后，第二个上下文失败，工作单元已经产生部分提交。
- 风险等级：`P0`，`严重bug｜逻辑缺陷`。
- 复现条件：注册两个 DbContext，使第一个写入成功，第二个遇到唯一键/外键冲突、数据库断连、超时或取消。
- 根因分析：工作单元只编排保存顺序，没有共享连接/事务，也没有跨库一致性协议；“所有 DbContext 保存完成后”只是调用顺序，不是原子性保证。
- 参考修复代码（仅适用于同一数据库和同一连接）：

```csharp
await using var transaction = await first.Database
    .BeginTransactionAsync(cancellationToken);

try
{
    foreach (var context in contexts)
    {
        context.Database.UseTransaction(transaction.GetDbTransaction());
        await context.SaveChangesAsync(cancellationToken);
    }

    await transaction.CommitAsync(cancellationToken);
}
catch
{
    await transaction.RollbackAsync(CancellationToken.None);
    throw;
}
```

- 方案取舍：同库、同连接可以共享本地事务，性能和一致性较好，但要求所有上下文使用相同连接。跨库不能假设本地事务原子，应改用 Outbox、Saga 或补偿事务，并让调用方明确最终一致性。

### 4. MySQL 迁移无条件清除所有建表外键

- 位置：[ServiceCollectionExtensions.cs:34-38](/Users/lewis/github/MSFramework/src/MSFramework.Ef.MySql/ServiceCollectionExtensions.cs:34)、[ServiceCollectionExtensions.cs:59-68](/Users/lewis/github/MSFramework/src/MSFramework.Ef.MySql/ServiceCollectionExtensions.cs:59)、[MigrationsSqlGenerator.cs:29-78](/Users/lewis/github/MSFramework/src/MSFramework.Ef.MySql/MigrationsSqlGenerator.cs:29)
- 问题描述：`Load()` 无条件调用 `UseRemoveExternalEntityService()`，设置静态 `RemoveForeignKey = true`。生成器随后对每个 `CreateTableOperation` 执行 `ForeignKeys.Clear()`，没有判断实体是否是外部实体。因此新建数据库的迁移脚本不会创建任何建表外键。
- 风险等级：`P0`，`严重bug｜逻辑缺陷｜并发安全`。数据库失去引用完整性，脏数据可以写入；静态开关还会污染同进程中的其他 DbContext、测试和并发迁移。
- 复现条件：使用 MySQL provider 生成包含普通订单、订单项关系的初始迁移；或在同一进程先加载该 provider，再生成本应保留外键的其他上下文迁移。
- 根因分析：把“排除外部实体”实现成 provider 级全局布尔开关，并在所有建表操作上统一清空外键；开关既没有按 DbContext 作用域隔离，也没有按操作目标筛选。
- 参考修复代码：

```csharp
// 首选：在实体配置中排除外部实体表，而不是改写普通表的 FK。
if (typeof(IExternalEntity).IsAssignableFrom(entityType.ClrType))
{
    entityType.SetIsTableExcludedFromMigrations(true);
}

// 若必须自定义生成器，只针对外部实体对应的表操作。
if (operation is CreateTableOperation create
    && IsExternalEntityTable(create.Name, model))
{
    create.ForeignKeys.Clear();
}
```

- 方案取舍：`SetIsTableExcludedFromMigrations` 依赖 EF 模型元数据，语义清晰且无全局状态；自定义生成器可以兼容更复杂的迁移规则，但需要可靠地把表名映射回实体。无论采用哪种方案，都应删除静态 `RemoveForeignKey`，或将状态改成每个 options 实例携带的不可变配置。

### 5. Newtonsoft 流序列化接口未实现，并关闭调用方输入流

- 位置：[NewtonsoftJsonSerializer.cs:57-90](/Users/lewis/github/MSFramework/src/MSFramework.Serialization.Newtonsoft/NewtonsoftJsonSerializer.cs:57)
- 问题描述：`Serialize<TValue>(Stream, TValue)` 直接抛出 `NotImplementedException`。`Deserialize<T>(Stream)` 使用默认 `StreamReader`，方法返回时会关闭调用方传入的流，破坏接口调用方的资源所有权。
- 风险等级：`P1`，`严重bug｜资源泄漏/资源所有权`。
- 复现条件：任何通过 `IJsonSerializer` 的流重载写 JSON；或在同一个 `MemoryStream`/网络流上连续反序列化后继续读取或写入。
- 根因分析：流重载停留在占位实现，且未明确 `leaveOpen` 契约；字符串重载可用掩盖了流路径未测试的问题。
- 参考修复代码：

```csharp
public void Serialize<T>(Stream output, T value)
{
    ArgumentNullException.ThrowIfNull(output);
    using var writer = new StreamWriter(
        output, new UTF8Encoding(false), 1024, leaveOpen: true);
    JsonSerializer.Create(_settings).Serialize(writer, value);
    writer.Flush();
}

public T Deserialize<T>(Stream input)
{
    ArgumentNullException.ThrowIfNull(input);
    using var reader = new StreamReader(
        input, Encoding.UTF8, true, 1024, leaveOpen: true);
    return JsonSerializer.Create(_settings).Deserialize<T>(reader);
}
```

- 方案取舍：默认 `leaveOpen: true` 适合通用序列化抽象，调用方若希望序列化器拥有流可单独提供带 ownership 参数的重载。也可以使用 `JsonTextReader/JsonTextWriter` 做真正的流式处理，避免先构造完整字符串；实现复杂度略高，但适合大 JSON。

## 逻辑缺陷

### 6. 领域事件快照丢失顺序、去重独立事件，且发布时机早于数据库提交

- 位置：[DbContextBase.cs:175-205](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:175)、[DbContextBase.cs:421-431](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:421)、[EntityBase.cs:13-42](/Users/lewis/github/MSFramework/src/MSFramework/Domain/EntityBase.cs:13)
- 问题描述：聚合内部使用 `List<DomainEvent>`，但 `GetDomainEvents()` 合并到 `HashSet<DomainEvent>`。两个独立发生但 record 值相同的事件会被合并；跨聚合的事件也可能按值去重。`HashSet` 不应被当作业务发生顺序。Handler 在当前快照建立后新增的事件不会在本次循环发布；如果后续没有再次保存，事件会一直留在实体中。更重要的是，当前在 `base.SaveChangesAsync` 前执行 Handler，外部 HTTP、消息、缓存等副作用可能先成功，数据库随后回滚。
- 风险等级：`P1`，`逻辑缺陷｜边界case遗漏｜异常处理`。
- 复现条件：连续两次添加字段完全相同的 record 事件；两个聚合产生相同事件；Handler 在执行时向另一个聚合添加事件；Handler 调用外部系统后数据库保存失败。
- 根因分析：把事件集合建模为值集合而不是发生序列，并且没有定义本次保存是否递归收集新事件；同步领域事件、数据库事务和跨进程副作用没有分层。
- 参考修复代码：

```csharp
var events = changeTracker.Entries<EntityBase>()
    .SelectMany(entry => entry.Entity.DequeueDomainEvents())
    .ToList(); // List 保留实例和发生顺序

foreach (var domainEvent in events)
{
    AddOutboxMessage(domainEvent); // 不在数据库提交前调用外部副作用
}

await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
```

- 方案取舍：Outbox 同时解决顺序、重试和提交后投递；若保留同步 Handler，应明确“Handler 只能修改当前事务内数据”，并对同一次 SaveChanges 中新增事件采用队列循环或明确延期到下一次保存。按引用实例去重可以保留两个相同值事件，但会牺牲原有的去重便利。

### 7. 本地事件失败即丢失，停机时不能可靠排空

- 位置：[LocalEventBackgroundService.cs:41-49](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:41)、[LocalEventBackgroundService.cs:96-147](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:96)、[LocalEventBackgroundService.cs:156-161](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:156)、[LocalEventChannel.cs:8-17](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventChannel.cs:8)
- 问题描述：事件一旦被 `TryRead` 取出就没有 ack；Handler 异常只记录日志，事件不会重试或进入死信。停止时先完成 Writer，再调用 `base.StopAsync`，后台执行通常会收到停止 token，积压事件可能在消费完成前被取消。队列只在进程内存中，进程崩溃会全部丢失。
- 风险等级：`P1`，`逻辑缺陷｜资源可靠性`。
- 复现条件：Handler 临时访问数据库/网络失败；服务重启时 Channel 尚有积压；事件发布成功后进程崩溃。
- 根因分析：内存 Channel 被当作可靠队列使用，消费模型缺少处理状态、重试计数、死信和优雅停机阶段。
- 参考修复代码：

```csharp
try
{
    await HandleAsync(entry, cancellationToken);
    await AckAsync(entry); // durable queue 中确认
}
catch (Exception ex) when (retryPolicy.CanRetry(entry, ex))
{
    await RequeueAsync(entry, retryPolicy.NextDelay(entry));
}
catch (Exception ex)
{
    await DeadLetterAsync(entry, ex);
}
```

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
{
    _eventChannel.EventChannel.Writer.TryComplete();
    await _consumerTask.WaitAsync(cancellationToken); // 先允许已入队事件排空
}
```

- 方案取舍：Outbox/持久化消息队列提供崩溃恢复，成本是存储、投递和幂等设计；若本地事件只用于非关键通知，可以保留内存 Channel，但应在文档中明确 best-effort，并增加有限重试、死信日志和停机宽限期。停机排空必须有最大等待时间，避免关闭无限阻塞。

### 8. SavedChanges 接口变更未兼容模板和既有消费者

- 位置：[IUnitOfWork.cs:10-22](/Users/lewis/github/MSFramework/src/MSFramework/Domain/IUnitOfWork.cs:10)、[EfUnitOfWork.cs:27-45](/Users/lewis/github/MSFramework/src/MSFramework.Ef/EfUnitOfWork.cs:27)、[template/Content/src/Template.Application/Project/V10/DomainEventHandlers/ProjectCreatedEventHandler.cs:20](/Users/lewis/github/MSFramework/template/Content/src/Template.Application/Project/V10/DomainEventHandlers/ProjectCreatedEventHandler.cs:20)
- 问题描述：`SavedChanges` 从同步 `Action` 形态改为 `Func<CancellationToken, ValueTask>`。当前 `EfUnitOfWork` 已正确使用 `GetInvocationList()` 逐个等待，旧报告中“只等待最后一个回调”的问题已关闭；但模板仍使用 `() => daprClient.PublishEventAsync(...)`，无法匹配新委托签名，旧调用者和已编译二进制也不兼容。
- 风险等级：`P1`，`逻辑缺陷｜工程规范｜兼容性`。
- 复现条件：编译模板项目；外部应用继续按旧的无参数 lambda 订阅 `SavedChanges`；升级 NuGet 后不重新编译调用方。
- 根因分析：改变了公开接口事件类型，却没有保留旧签名、提供迁移适配层或同步更新模板。
- 参考修复代码：

```csharp
// 保留旧 API，新增明确的异步 API，避免改变既有二进制契约。
event Action SavedChanges;
event Func<CancellationToken, ValueTask> SavedChangesAsync;

// 新模板调用
unitOfWork.SavedChangesAsync += cancellationToken =>
    daprClient.PublishEventAsync("pubsub", "topic", data, cancellationToken);
```

- 方案取舍：保留旧事件的兼容性最好，但同步回调无法表达异步失败；新增异步事件是推荐方案。若必须只保留一个事件，应提供 `RegisterSavedChanges(Func<CancellationToken, ValueTask>)` 方法并在同一主版本更新模板、迁移文档和包版本，不能把接口破坏伪装成内部重构。

### 9. 外部实体缓存命中时仍执行工厂

- 位置：[ExternalEntityRepository.cs:35-56](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/ExternalEntityRepository.cs:35)
- 问题描述：`Load(Func<TEntity>)` 先执行 `factory()`，再将实体交给缓存。即使命中缓存，工厂仍会执行；如果工厂包含 HTTP、数据库访问、反序列化或副作用，命中缓存没有节省成本。
- 风险等级：`P2`，`逻辑缺陷｜性能隐患`。
- 复现条件：相同外部实体重复调用 `Load(() => FetchFromRemote())`；缓存命中但远程接口仍被调用。
- 根因分析：接口只接收无 key 的工厂，无法在执行工厂前定位缓存条目；`ConcurrentDictionary.GetOrAdd` 也不保证并发工厂只执行一次。
- 参考修复代码：

```csharp
private readonly ConcurrentDictionary<TKey, Lazy<TEntity>> _cache = new();

public TEntity Load(TKey key, Func<TEntity> factory)
{
    ArgumentNullException.ThrowIfNull(factory);
    var lazy = _cache.GetOrAdd(key, _ =>
        new Lazy<TEntity>(factory, LazyThreadSafetyMode.ExecutionAndPublication));
    return lazy.Value;
}
```

- 方案取舍：新增 key 参数并使用 `Lazy<TEntity>` 可避免命中时执行工厂，也能控制并发重复创建；代价是改变接口和需要处理工厂抛异常后的缓存策略。若不能改接口，应明确要求 factory 纯函数且接受重复执行，或改用 `TryGet` 后再创建并接受竞态。

### 10. Owned/Keyless 实体仍被属性级约定处理

- 位置：[ModelBuildingStrategies.cs:131-173](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Internal/ModelBuildingStrategies.cs:131)
- 问题描述：`EntityTableConventionStrategy` 会跳过 Owned/Keyless，但 `EntityPropertyConventionStrategy` 当前没有相同过滤。注释声称“遍历所有非 Owned、非 Keyless”，实际仍可能给查询模型或 Owned 属性改列名、长度、Guid/ObjectId converter、Enumeration converter 和默认审计字段。
- 风险等级：`P1/P2`，`逻辑缺陷｜边界case遗漏`。
- 复现条件：定义 `HasNoKey()`/`ToView()` 的读取模型，或定义包含 ObjectId、Enumeration 的 Owned 类型，并启用自动模型约定；检查生成 SQL 或模型元数据发现列类型被自动替换。
- 根因分析：重构策略时删除了实体循环顶部的 `IsOwned()`/`IsKeyless` guard，只保留了列名方法内部对无表名的部分保护；列类型和审计默认值路径没有保护。
- 参考修复代码：

```csharp
foreach (var entityType in context.ModelBuilder.Model.GetEntityTypes())
{
    if (entityType.IsOwned() || entityType.IsKeyless)
    {
        continue;
    }

    // 只对普通实体应用属性级约定。
    ApplyProperties(entityType, context);
}
```

- 方案取舍：统一跳过最简单、行为最可预测；若某些 Owned 类型确实需要自动 converter，应改成显式 opt-in，而不是对所有 Owned/Keyless 类型全局套用。

### 11. DateTime 与 DateTimeOffset 的非法/未指定时区语义不一致

- 位置：[DateTimeJsonConverter.cs:27-65](/Users/lewis/github/MSFramework/src/MSFramework/Text/Json/Converters/DateTimeJsonConverter.cs:27)、[DateTimeOffsetJsonConverter.cs:19-45](/Users/lewis/github/MSFramework/src/MSFramework/Text/Json/Converters/DateTimeOffsetJsonConverter.cs:19)、[DateTimeExtensions.cs:19-29](/Users/lewis/github/MSFramework/src/MSFramework/Extensions/DateTimeExtensions.cs:19)
- 问题描述：`DateTime` 数字读取为 UTC；写入时 `DateTimeKind.Unspecified` 又按本地时区转换。这样墙钟值经过数字往返后 `Kind` 和语义可能改变。`DateTimeOffset` 字符串使用当前文化的 `Parse`，空字符串静默变为 `MinValue`，与 `DateTime` 转换器对空/非法输入抛 `JsonException` 的策略不一致。
- 风险等级：`P2`，`逻辑缺陷｜边界case遗漏`。
- 复现条件：在非 UTC 服务器上序列化 `DateTime.SpecifyKind(value, Unspecified)` 后反序列化；切换线程文化后解析同一个带数字日期字符串；传入空 DateTimeOffset 字符串。
- 根因分析：`DateTime` 的数据库本地墙钟兼容策略与 Unix 时间戳的 UTC 约定混在同一转换器中；两个转换器没有共享格式、文化和非法输入契约。
- 参考修复代码：

```csharp
// 统一约定：JSON 数字永远表示 UTC Unix seconds。
return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

// 字符串也使用不变文化，并拒绝空值/非法值。
if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
        DateTimeStyles.RoundtripKind, out var result))
{
    throw new JsonException("Invalid DateTimeOffset value");
}
return result;
```

- 方案取舍：统一 UTC 最简单，适合跨服务数据；若业务必须保留本地墙钟，应使用带 offset 的 ISO 8601 字符串或单独的 LocalDateTime 类型，不能依赖服务器时区隐式解释 `Unspecified`。

## 性能隐患

### 12. EF 仓储默认 Include 所有一级导航

- 位置：[EfRepository.cs:31-89](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/EfRepository.cs:31)、[EfRepository.cs:199-202](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/EfRepository.cs:199)
- 问题描述：首次访问 `Store` 时自动 Include 所有一级导航，任何 `Find`、分页和派生仓储查询都会继承该行为。导航超过 3 个才启用 SplitQuery，SplitQuery 只能降低笛卡尔积，不能消除不必要的网络传输、实体物化和跟踪开销。按主键删除也通过 `Find(id)` 走 `Store`，与“删除不加载导航”的注释不一致。
- 风险等级：`P1`，`性能隐患`。
- 复现条件：聚合包含多个大集合导航，调用只需要 Id 的 `Find`/`Delete`；观察生成 SQL、返回行数、内存和数据库耗时。
- 根因分析：仓储基类把“完整聚合读取”硬编码成所有查询的默认行为，读模型、点查、删除和分页没有查询粒度区分。
- 参考修复代码：

```csharp
protected virtual IQueryable<TEntity> Store => _dbSet;

public virtual Task<TEntity> FindAsync(TKey id, CancellationToken cancellationToken = default)
{
    return _dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
}

// 需要完整聚合时由调用方显式 Include。
var order = repository.Query()
    .Include(x => x.Items)
    .SingleAsync(x => x.Id == id, cancellationToken);
```

- 方案取舍：默认不 Include 可显著降低普通查询成本，但要求写操作或领域服务显式声明需要的导航；保留自动 Include 兼容性较好，却无法从基类统一控制大聚合，至少应提供 `IncludeNavigations`/`AsAggregate` 两种明确模式。

### 13. 分页缺少最大 limit、稳定排序和一致性读取

- 位置：[PagedQueryExtensions.cs:35-82](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/PagedQueryExtensions.cs:35)
- 问题描述：`limit` 只把小于 1 的值改为 10，没有最大值限制；调用方可以传 `int.MaxValue`，导致巨大查询和内存分配。扩展方法没有强制 `OrderBy`，数据库无序结果在插入/删除并发下会跨页重复或遗漏。`CountAsync` 和 `ToListAsync` 是两次独立查询，total 与 data 可能来自不同时间点。
- 风险等级：`P1`，`性能隐患｜逻辑缺陷｜边界case遗漏`。
- 复现条件：传入 `int.MaxValue`；不排序后并发插入数据；在 Count 和数据查询之间执行另一事务的写入或删除。
- 根因分析：分页 API 没有把页大小上限、排序键和一致性级别作为契约的一部分；数据库查询自然顺序不是分页稳定性保证。
- 参考修复代码：

```csharp
page = Math.Max(page, 1);
limit = Math.Clamp(limit, 1, maxPageSize);

var ordered = queryable.OrderBy(orderKey); // API 要求调用方提供稳定键
var total = await ordered.CountAsync(cancellationToken);
var data = await ordered.Skip((page - 1) * limit)
    .Take(limit)
    .ToListAsync(cancellationToken);
```

- 方案取舍：限制大小和要求排序是低复杂度、收益高的修复；若 total 必须和数据严格一致，应在同一事务/快照隔离级别中读取，或使用数据库窗口函数一次返回总数，前者增加锁/事务成本，后者增加 SQL 和 provider 兼容复杂度。

### 14. JSON ValueComparer 每次比较、Hash 和快照都序列化

- 位置：[JsonPropertyBuilderExtensions.cs:48-94](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs:48)
- 问题描述：当前已正确在模型属性上设置了 `ValueComparer`，旧报告“没有设置 Model ValueComparer”的问题已关闭；但 `ContentEquals` 序列化两次，`ComputeHashCode` 再序列化，Snapshot 又序列化后反序列化。大 JSON 或高频变更跟踪场景会产生重复 CPU、字符串和对象分配。序列化结果还会受字典插入顺序和全局 serializer 设置影响，无序字典可能被误判为变化。
- 风险等级：`P1/P2`，`性能隐患｜边界case遗漏`。
- 复现条件：实体包含几十 KB 以上的可变 JSON 属性并被 EF 长时间跟踪；两个内容相同但插入顺序不同的字典被赋值；运行时替换 `Defaults.JsonSerializer`。
- 根因分析：比较器把 JSON 文本格式当作内容等价标准；每个 EF comparer 回调都重新序列化，且 comparer 闭包读取可变的全局序列化器。
- 参考修复代码：

```csharp
// 推荐：对有明确结构的值对象实现结构化、不可变比较。
var comparer = new ValueComparer<MyJsonValue>(
    (left, right) => left?.Equals(right) ?? right is null,
    value => value?.GetHashCode() ?? 0,
    value => value is null ? null : value with { });

property.Metadata.SetValueComparer(comparer);
```

```csharp
// 必须使用 JSON 时，固定 serializer，并对对象键进行规范化后再比较。
var canonical = CanonicalJson.Serialize(value, fixedOptions);
```

- 方案取舍：结构化 comparer 性能和语义最好，但需要为每种 JSON 类型维护比较逻辑；规范化 JSON 兼容任意结构，却仍有序列化成本。至少应在模型构建时捕获不可变 serializer 配置，并检查 `fieldType.IsAssignableTo(typeof(TProperty))`，避免运行时强制转换失败。

## 并发安全

### 15. 上传文件使用预检查和可共享写入，存在同一目标竞争

- 位置：[FormFileExtensions.cs:110-145](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:110)、[FormFileExtensions.cs:209-227](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:209)
- 问题描述：`File.Exists(dedupeLinkPath)` 后再写入和建链是典型 TOCTOU；真实文件使用 `FileMode.Create` 与 `FileShare.ReadWrite`，其他请求可能在文件尚未写完时读取，多个进程也可能同时截断同一目标。符号链接失败后的 `File.Copy(..., overwrite: true)` 不是原子替换，崩溃或并发时可能留下半成品。
- 风险等级：`P1`，`并发安全｜数据完整性`。
- 复现条件：并发上传相同内容；静态文件请求与写入同时发生；文件系统不支持符号链接或进程在 Copy 中途崩溃。
- 根因分析：目标文件直接作为临时写入位置，缺少 `CreateNew`、临时文件、原子 rename 和跨进程唯一性控制；`FileShare.ReadWrite` 把未完成状态暴露给读者。
- 参考修复代码：

```csharp
var tempPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
await using (var output = new FileStream(
    tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
    64 * 1024, FileOptions.Asynchronous))
{
    await input.CopyToAsync(output, cancellationToken);
}

// 同一卷内 Move 是原子可见的；目标已存在时复用已确认的完整文件。
try
{
    File.Move(tempPath, targetPath);
}
catch (IOException) when (File.Exists(targetPath))
{
    File.Delete(tempPath);
}
```

- 方案取舍：临时文件加原子 Move 能解决单机/同卷并发，数据库唯一键或分布式锁才适合多节点共享存储。保留符号链接可节省空间，但跨平台权限和降级复制逻辑更复杂；复制模式至少应先写临时副本再原子替换。

### 16. Defaults 和 Runtime 全局状态会被多个宿主/并行测试覆盖

- 位置：[Defaults.cs:17-54](/Users/lewis/github/MSFramework/src/MSFramework/Defaults.cs:17)、[ServiceCollectionExtensions.cs:66-80](/Users/lewis/github/MSFramework/src/MSFramework/ServiceCollectionExtensions.cs:66)、[Runtime.cs:22-36](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Runtime.cs:22)
- 问题描述：`Defaults.JsonSerializer`、`Defaults.Logger` 是可写静态属性，`Defaults.ServiceProvider` 持有整个根 DI 图。两个 Host、并行测试或不同容器先后调用 `UseMicroserviceFramework` 时会互相覆盖；同一请求期间若 serializer 被替换，JSON comparer、审计和事件日志可能使用不同语义。
- 风险等级：`P1/P2`，`并发安全｜工程规范`。
- 复现条件：在同一进程启动两个应用服务提供者；并行测试分别设置不同 serializer/logger；一个后台事件执行期间另一个 Host 初始化框架。
- 根因分析：依赖运行时上下文的对象通过全局可变变量传递，生命周期、所有权和覆盖规则没有定义；静态 Provider 还会延长 scoped/singleton 图的可达时间。
- 参考修复代码：

```csharp
services.AddSingleton<IJsonSerializer>(sp =>
    sp.GetService<JsonSerializerOptions>() is { } options
        ? new TextJsonSerializer(options)
        : TextJsonSerializer.Create());

public sealed class AuditService(IJsonSerializer serializer, ILogger<AuditService> logger)
{
    // 通过构造函数获得当前 Host 的依赖，不读取 Defaults 静态状态。
}
```

- 方案取舍：彻底移除全局状态最可靠，但涉及较多 API 传递；过渡方案可以把 serializer/logger 设为“只初始化一次且不可替换”，仍不能支持同进程多 Host。`ServiceProvider` 不应作为全局服务定位器，应通过 DI 或显式 scope 传递。

### 17. `ExistingDirCache` 无界增长

- 位置：[FormFileExtensions.cs:17-21](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:17)、[FormFileExtensions.cs:160-198](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:160)
- 问题描述：静态 `ConcurrentDictionary` 记录所有曾成功创建过的完整目录，没有过期、容量上限或删除失效项。日期、租户、用户自定义 interval 或多节点部署会不断产生新 key，进程生命周期内内存持续增长。
- 风险等级：`P2`，`并发安全｜资源泄漏｜性能隐患`。
- 复现条件：每天生成新日期目录，或批量调用不同 interval；长期运行进程后检查字典大小和内存。
- 根因分析：把目录存在性缓存当成永久事实，但目录可能被外部删除、挂载切换或权限改变；静态容器没有生命周期边界。
- 参考修复代码：

```csharp
// 目录创建本身是幂等操作，优先删除永久缓存。
Directory.CreateDirectory(fullPath);

// 若确有收益，使用有界、带过期的 MemoryCache，命中后仍按失败重试。
cache.Set(fullPath, true, new MemoryCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(10),
    Size = 1
});
```

- 方案取舍：直接 `CreateDirectory` 通常足够，代码最简单；有界缓存可减少高频目录探测，但需要处理删除、失效和容量配置，不能用永久静态字典。

## 安全风险

### 18. 文件上传写入 `wwwroot`，危险扩展名黑名单不可作为安全边界

- 位置：[FormFileExtensions.cs:23-42](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:23)、[FormFileExtensions.cs:73-108](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:73)
- 问题描述：上传内容位于 `AppContext.BaseDirectory/wwwroot`，通常会被静态文件中间件直接访问。当前只黑名单一组后缀，未限制白名单、MIME、文件内容和单文件大小，仍可上传 `.xhtml`、`.xml`、`.mjs`、`.json`、`.wasm` 或服务器特定可执行格式。`interval` 为空时还会在 `Contains` 处抛 NRE。
- 风险等级：`P1`，`安全风险｜边界case遗漏`。风险取决于宿主静态文件配置，但一旦允许脚本或主动内容被浏览器解释，可能导致存储型 XSS 或恶意文件托管。
- 复现条件：上传合法扩展名但内容为 HTML/JavaScript；上传超大文件耗尽磁盘或内存；传入 `interval: null`；将 `wwwroot/upload` 暴露为静态目录。
- 根因分析：以扩展名黑名单替代内容安全策略，存储目录与公开静态目录耦合，API 没有大小和内容校验。
- 参考修复代码：

```csharp
ArgumentException.ThrowIfNullOrWhiteSpace(interval);

var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { ".jpg", ".jpeg", ".png", ".pdf", ".txt" };
if (!allowedExtensions.Contains(extension)
    || formFile.Length <= 0
    || formFile.Length > options.MaxFileLength)
{
    throw new InvalidDataException("Unsupported or oversized upload");
}

var storageRoot = Path.GetFullPath(options.StorageRoot); // 位于 webroot 外
// 根据文件类型做 magic number/content 校验，并通过下载端点设置安全 Content-Type。
```

- 方案取舍：白名单加内容检测和 webroot 外存储风险最低，但限制了业务文件类型；若必须直接静态访问，至少禁用脚本/主动内容 MIME、设置 `Content-Disposition: attachment` 和 CSP。大小限制应同时在 Kestrel/FormOptions 与本方法设置，避免只依赖单层配置。

### 19. 使用 MD5 作为文件身份，存在碰撞和扩展名混淆风险

- 位置：[FormFileExtensions.cs:92-108](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:92)、[FormFileExtensions.cs:112-147](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:112)
- 问题描述：文件名和判重 key 由 MD5 与上传扩展名组成。MD5 不适合作为安全完整性身份，攻击者可构造碰撞；同内容使用不同扩展名会生成不同路径，内容类型也完全由客户端扩展名决定。若系统将 `PhysicalPath` 当作可信内容身份或授权依据，可能发生内容替换、错误复用和审计追踪失真。
- 风险等级：`P1`，`安全风险｜数据完整性`。
- 复现条件：攻击者上传已知 MD5 碰撞文件；同一字节内容分别以 `.txt` 和 `.html` 上传；业务使用返回路径判断“文件未被替换”。
- 根因分析：将快速去重摘要误当作抗碰撞的安全标识，且没有存储实际字节长度、强哈希、媒体类型和授权元数据。
- 参考修复代码：

```csharp
using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
await CopyAndHashAsync(input, tempStream, hasher, cancellationToken);
var contentHash = Convert.ToHexString(hasher.GetHashAndReset());

var identity = new FileIdentity(contentHash, actualContentType, length);
// 去重命中后仍校验 hash、长度和内容类型，不以客户端文件名作为安全依据。
```

- 方案取舍：SHA-256 的 CPU 成本略高但足够适合作为内容身份；若只为非安全场景做近似去重，MD5 可以保留为性能索引，但必须同时保存 SHA-256/长度，并禁止把 MD5 作为授权、签名或完整性判断。

### 20. AES 默认 ECB、无认证标签，且直接把字符串当作 Key

- 位置：[Cryptography.cs:86-124](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Cryptography.cs:86)
- 问题描述：`CreateAes` 默认 `CipherMode.ECB`，ECB 会泄露重复明文块；API 只使用 AES-CBC/ECB 风格的加解密，没有认证标签，密文可被篡改后再解密；UTF-8 字节直接作为 AES key，密码长度和熵没有经过 KDF，空值/不安全短密码也没有统一校验。
- 风险等级：`P1`，`安全风险`。
- 复现条件：对包含重复结构的长文本加密并观察密文块；篡改 Base64 密文后调用解密；使用低熵字符串作为 key；跨服务使用不同平台时遇到 key 长度异常。
- 根因分析：通用工具暴露了低级 AES 参数并将默认值设为 ECB，缺少 nonce、tag、密钥派生和版本化密文格式。
- 参考修复代码：

```csharp
var key = Rfc2898DeriveBytes.Pbkdf2(
    password, salt, iterations: 600_000,
    HashAlgorithmName.SHA256, key.Length);
var nonce = RandomNumberGenerator.GetBytes(12);
var tag = new byte[16];
var cipher = new byte[plain.Length];

using var aes = new AesGcm(key, tag.Length);
aes.Encrypt(nonce, plain, cipher, tag, associatedData: versionBytes);
// 输出：版本 | salt | nonce | tag | cipher；解密失败必须拒绝，不返回部分明文。
```

- 方案取舍：AES-GCM 提供机密性和完整性，推荐用于新数据；若运行环境不支持 GCM，可使用随机 IV 的 CBC 加 HMAC，但实现和密钥管理更复杂。旧 ECB 数据应保留兼容解密后在线迁移，不能直接改变默认格式导致历史数据不可读。

### 21. 审计与异常日志记录完整 URL、QueryString 和实体原值

- 位置：[AuditAtrribute.cs:182-207](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Filters/AuditAtrribute.cs:182)、[LokiAuditingStore.cs:77-120](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:77)、[GlobalExceptionFilter.cs:47-89](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Filters/GlobalExceptionFilter.cs:47)
- 问题描述：审计对象保存完整 `GetDisplayUrl()`、QueryString、User-Agent、IP、设备标识和位置数据；Loki 又把每个实体属性的 Original/New 值全部写入日志。QueryString 常包含 token、password、code 等秘密，实体字段可能包含身份证、手机号、凭据或业务隐私。异常日志还将完整 URL 和异常对象写入日志系统。
- 风险等级：`P1`，`安全风险｜隐私合规`。
- 复现条件：请求 URL 含 `access_token`/`password` 查询参数；修改包含敏感字段的实体并启用 Loki；查询 Loki 或本地异步日志文件。
- 根因分析：审计采用全量采集和动态属性名，没有字段 allowlist、敏感名脱敏、长度上限或日志访问分级；URL 日志没有剥离 QueryString。
- 参考修复代码：

```csharp
var safePath = request.Path.Value;
var safeQuery = RedactQuery(request.Query,
    "token", "access_token", "password", "secret", "code");

foreach (var property in auditEntity.Properties)
{
    if (!auditPolicy.AllowedProperties.Contains(property.Name))
    {
        continue;
    }

    properties.Add(new LogEventProperty(property.Name,
        new ScalarValue(TruncateAndRedact(property.NewValue))));
}
```

- 方案取舍：字段 allowlist 和统一脱敏策略最安全，但需要业务维护字段分类；最低限度应去掉 QueryString、限制日志值长度、对 token/password 等名称做强制脱敏，并将完整审计放到受控数据库而不是普通日志平台。异常日志保留 TraceId 和异常类型即可，堆栈应按访问权限存储。

## 资源泄漏

### 22. Loki logger 未释放，异步 sink 的尾部日志没有确认语义

- 位置：[LokiAuditingStore.cs:17-60](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:17)、[LokiAuditingStore.cs:68-133](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:68)、[ServiceCollectionExtensions.cs:19-26](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/ServiceCollectionExtensions.cs:19)
- 问题描述：`Create()` 创建 Serilog logger，并挂载 `WriteTo.Async`，但 `LokiAuditingStore` 不持有可释放契约，也未实现 `IDisposable`。`AddAsync` 和额外的 `CommitAsync` 都立即返回 `CompletedTask`，调用方无法知道 Loki/文件异步 sink 是否已经接收；应用停止时可能丢失队列尾部日志。
- 风险等级：`P1/P2`，`资源泄漏｜逻辑缺陷`。
- 复现条件：多次创建/销毁 Host；短进程写审计后立即退出；网络不可用或 Async sink 队列积压时查询 Loki。
- 根因分析：logger 生命周期没有绑定到 DI singleton 的释放周期，异步 sink 的 flush 被隐藏在一个不参与接口契约的同步返回值后面；`IAuditingStore` 本身也没有 flush/close 语义。
- 参考修复代码：

```csharp
public sealed class LokiAuditingStore : IAuditingStore, IDisposable
{
    private readonly Serilog.ILogger _logger;

    public void Dispose()
    {
        (_logger as IDisposable)?.Dispose(); // 触发 sink flush/释放句柄
    }
}
```

```csharp
// 若调用方要求提交确认，应把接口改成真正异步的 durable write/flush；
// 若只能 best-effort，则删除伪 CommitAsync，并在文档中明确不保证落库。
```

- 方案取舍：让 store 实现 `IDisposable` 是低成本必要修复；可靠审计需要 awaitable HTTP/队列写入或应用停止时统一 `Log.CloseAndFlush`，会增加请求延迟和失败处理。Serilog Async 适合吞吐优先的 best-effort 日志，不应被当作事务审计确认。

### 23. DesignTime 工厂创建的 Scope 没有释放

- 位置：[DesignTimeDbContextFactoryBase.cs:21-34](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DesignTimeDbContextFactoryBase.cs:21)
- 问题描述：`CreateDbContext` 为了让返回的 DbContext 继续可用，不能在方法内直接 `using` scope；当前实现因此把 `services.CreateScope()` 丢失引用，scope 只能等根 provider 或进程退出时回收。该上下文可能持有连接、日志和其他 scoped 资源。
- 风险等级：`P2`，`资源泄漏｜工程规范`。影响主要限于 `dotnet ef` 短生命周期进程，不应夸大为生产常驻泄漏。
- 复现条件：设计时工厂在同一进程中反复创建上下文或自定义工具长时间保持进程；检查 scoped disposable 对象未及时释放。
- 根因分析：返回的 DbContext 依赖创建它的 scope，而接口只返回 DbContext，没有把 scope 的生命周期一起交给调用方。
- 参考修复代码：

```csharp
// 优先使用设计时独立构造，避免通过临时 scope 解析 scoped DbContext。
var options = BuildDesignTimeOptions(args);
return ActivatorUtilities.CreateInstance<TDbContext>(
    serviceProvider, options);
```

- 方案取舍：直接构造或使用 `IDbContextFactory<T>` 最清晰，但要求上下文依赖可在设计时提供；若必须用 scope，可让自定义包装对象同时持有 DbContext 和 scope，并由 EF 工具侧负责释放，不能把 scope 丢弃。

### 24. 请求体读取没有函数级大小上限

- 位置：[HttpContextAccessorExtensions.cs:27-50](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs:27)
- 问题描述：当前已在 `finally` 恢复原位置，且 `StreamReader` 使用 `leaveOpen: true`，旧报告中“异常路径不恢复/reader 关闭 Body”的问题已基本关闭；但 `EnableBuffering` 后直接 `ReadToEndAsync()`，方法本身没有最大读取大小。若宿主未正确配置 Form/Kestrel 限制，单个请求可造成大量内存、临时文件和 CPU 消耗。
- 风险等级：`P1/P2`，`资源泄漏｜性能隐患｜边界case遗漏`。
- 复现条件：调用方对无大小限制的大请求调用该扩展；发送几十 MB 或更大的 body 并发请求。
- 根因分析：通用 helper 假定上游一定已完成请求大小限制，而 buffering 和文本物化会把整个 body 置于当前请求处理路径。
- 参考修复代码：

```csharp
if (request.ContentLength is > MaxBodyBytes)
{
    throw new BadHttpRequestException("Request body is too large");
}

using var reader = new StreamReader(
    request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
var text = await ReadAtMostAsync(reader, MaxBodyBytes, cancellationToken);
```

- 方案取舍：优先在服务器和 endpoint 层限制 body，再在 helper 做防御性上限；直接读字符串最方便，但大 payload 应改用流式解析或显式调用方选择，避免所有请求都全量 materialize。

## 工程规范

### 25. AutoMapper 扩展删除旧的程序集参数重载

- 位置：[ServiceCollectionExtensions.cs:21-55](/Users/lewis/github/MSFramework/src/MSFramework.AutoMapper/ServiceCollectionExtensions.cs:21)
- 问题描述：当前 `UseAutoMapperObjectAssembler` 的第一个重载变为 `Action<IMapperConfigurationExpression>`，第二个重载要求先传 configAction，再传 `params Assembly[]`。原有 `UseAutoMapperObjectAssembler(typeof(Profile).Assembly)` 调用无法编译；已编译消费者也会遇到公开方法签名缺失。模板只调用无参重载，不能证明旧程序集参数 API 兼容。
- 风险等级：`P1`，`工程规范｜兼容性`。
- 复现条件：下游项目继续使用上一版本的程序集扫描调用；或升级包后编译已有源码。
- 根因分析：为增加 AutoMapper 配置委托直接修改公共方法签名，没有保留旧的 `params Assembly[]` API 或提供明确的新增方法名。
- 参考修复代码：

```csharp
// 保留旧源码/二进制入口。
public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(
    this MicroserviceFrameworkBuilder builder, params Assembly[] assemblies)
{
    builder.Services.AddAutoMapperObjectAssembler(null, assemblies);
    return builder;
}

// 新 API 不把 configAction 设为可选，避免与无参/params 重载产生歧义。
public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(
    this MicroserviceFrameworkBuilder builder,
    Action<IMapperConfigurationExpression> configAction,
    params Assembly[] assemblies)
{
    builder.Services.AddAutoMapperObjectAssembler(configAction, assemblies);
    return builder;
}
```

- 方案取舍：保留旧重载并新增重载对源码迁移最友好；二进制兼容仍需保留原始元数据签名并按主版本管理。也可以只保留一个带 options 的新方法，但必须升主版本、提供迁移文档并同步模板。

### 26. 编译模型 API 被注释，文档仍宣传已不存在的方法

- 位置：[DbContextOptionsBuilderExtensions.cs:1-66](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/DbContextOptionsBuilderExtensions.cs:1)、[DbContextSettings.cs:127-130](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextSettings.cs:127)、[README.md:559-566](/Users/lewis/github/MSFramework/README.md:559)
- 问题描述：`LoadModel` 扩展整文件被注释，实际不再提供；`DbContextSettings.UseCompiledModel` 仍公开且标记 obsolete；README 仍指导用户调用 `options.LoadModel(...)`。用户按文档配置会直接编译失败，或误以为设置 `UseCompiledModel` 已生效。
- 风险等级：`P1/P2`，`工程规范｜兼容性`。
- 复现条件：按 README 创建编译模型并调用 `LoadModel`；搜索公共程序集发现配置属性存在但没有实际 options 行为。
- 根因分析：实现因与软删除/多租户不兼容被临时注释，但没有同步删除或重写公开设置和文档。
- 参考修复代码：

```csharp
// 方案 A：彻底移除过时设置并将 README 改为当前支持方式。
// 方案 B：恢复正式 API，并在模型构建阶段验证所有全局过滤器/租户约定。
public static DbContextOptionsBuilder UseCompiledModelSafe(
    this DbContextOptionsBuilder builder, IModel model)
{
    ArgumentNullException.ThrowIfNull(model);
    return builder.UseModel(model);
}
```

- 方案取舍：若框架不能保证编译模型和自动约定一致，删除设置与文档比保留半可用 API 更安全；若恢复，必须为软删除、租户过滤器、多个 DbContext 和设计时模型补齐集成测试。

### 27. 审计字段长度变更需要 migration/schema 兼容检查

- 位置：[AuditEntityConfiguration.cs:12-25](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Auditing/Configuration/AuditEntityConfiguration.cs:12)、[AuditOperationConfiguration.cs:13-28](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Auditing/Configuration/AuditOperationConfiguration.cs:13)、[AuditPropertyConfiguration.cs:8-15](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Auditing/Configuration/AuditPropertyConfiguration.cs:8)、[ModelBuildingStrategies.cs:302-390](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Internal/ModelBuildingStrategies.cs:302)
- 问题描述：多个审计字段默认长度从 256 改为 255；`AuditOperation.UserAgent` 则从 1024 改为无限长度。已部署数据库若仍是旧 schema，模型快照/迁移可能产生不必要的列变更；无限长度的 User-Agent 来源于不可信请求头，会放大审计存储和索引压力。该项不能仅以“生成模型成功”视为完成。
- 风险等级：`P1/P2`，`工程规范｜逻辑缺陷｜安全风险`。
- 复现条件：已有 256 长度字段的数据库执行新迁移；发送超长 User-Agent 并启用审计；不同 provider 对无长度字符串映射到 text/nvarchar(max) 后查询或索引行为改变。
- 根因分析：配置变更没有配套 migration、生产数据兼容策略和输入上限；审计字段同时承担存储和不可信请求采集。
- 参考修复代码：

```csharp
private const int AuditTextMaxLength = 1024;

builder.Property(x => x.UserAgent)
    .HasMaxLength(AuditTextMaxLength);

var userAgent = request.Headers.UserAgent.ToString();
audit.UserAgent = Truncate(userAgent, AuditTextMaxLength);
```

- 方案取舍：保持旧长度可避免无意义 schema 迁移；统一增加明确上限可以控制存储，但超长值会被截断，需记录策略。若确实要从 256 改 255 或改为 text，应先生成、审查并在各 provider 上验证 migration，不能只修改 model configuration。

### 28. 默认构建会隐式触发打包并放大并行构建竞争

- 位置：[package.props:1-23](/Users/lewis/github/MSFramework/package.props:1)、[MSFramework.Analyzers.csproj:8-15](/Users/lewis/github/MSFramework/src/MSFramework.Analyzers/MSFramework.Analyzers.csproj:8)、[MSFramework.Ef.Analyzers.csproj:5-9](/Users/lewis/github/MSFramework/src/MSFramework.Ef.Analyzers/MSFramework.Ef.Analyzers.csproj:5)
- 问题描述：公共 `package.props` 设置 `GeneratePackageOnBuild=true`，两个 analyzer 项目又显式设置该属性。普通 `dotnet build` 会同时执行 pack，容易在解决方案并行构建时争用包输出、增加耗时，并让“验证编译”产生发布副作用。
- 风险等级：`P2`，`工程规范｜性能隐患｜资源管理`。
- 复现条件：在干净环境执行默认 `dotnet build` 或解决方案级并行构建；观察同时生成 `.nupkg`、包输出目录和构建时间。
- 根因分析：把发布流程属性放在所有开发构建都会继承的公共 props 中，没有把 build 和 pack 生命周期分离。
- 参考修复代码：

```xml
<!-- package.props：默认只负责编译，不隐式 pack -->
<GeneratePackageOnBuild>false</GeneratePackageOnBuild>

<!-- 发布流水线显式执行 -->
dotnet pack --configuration Release --output artifacts/packages
```

- 方案取舍：拆分 build/pack 可提高本地和 CI 稳定性；若 analyzer 开发确实需要每次生成包，可仅在 analyzer 的发布 target 或显式 `-p:GeneratePackageOnBuild=true` 中启用，并使用独立输出目录。

### 29. 版本字符串不符合程序集版本格式并持续产生构建警告

- 位置：[version.props:1-5](/Users/lewis/github/MSFramework/version.props:1)
- 问题描述：`Version` 和 `FileVersion` 当前为 `0.15.5-beta01`。该格式适合作为 NuGet 版本标签，但 `FileVersion`/程序集版本属性要求由点分隔的数字版本，构建会对多个 MSFramework 项目产生 `CS7035` 警告。警告若被忽略，可能掩盖真正的编译诊断；若将同一字符串用于强命名/程序集版本，还可能导致工具链兼容性问题。
- 风险等级：`P2`，`工程规范`。
- 复现条件：执行任意 MSFramework 项目的 `dotnet build`，观察 `AssemblyInfo.cs` 对 `0.15.5-beta01` 的版本格式警告。
- 根因分析：NuGet 语义版本、文件版本和程序集版本共用一个 MSBuild 属性，没有分别配置；预发布标签没有被限制在 `PackageVersion`。
- 参考修复代码：

```xml
<VersionPrefix>0.15.5</VersionPrefix>
<VersionSuffix>beta01</VersionSuffix>
<PackageVersion>$(VersionPrefix)-$(VersionSuffix)</PackageVersion>
<Version>$(VersionPrefix)</Version>
<FileVersion>0.15.5.0</FileVersion>
<AssemblyVersion>0.15.5.0</AssemblyVersion>
```

- 方案取舍：分离包版本和程序集版本可消除警告并保持预发布包命名；若团队明确接受非数字文件版本，也可以仅抑制 `CS7035`，但会降低诊断质量，不建议作为默认修复。

### 30. ResponseWrapper 测试契约未随生产修复同步

- 位置（验证范围外的测试）：[ResponseWrapperFilter.cs:60-87](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Filters/ResponseWrapperFilter.cs:60)、[ResponseWrapperFilterTests.cs:137-315](/Users/lewis/github/MSFramework/tests/MSFramework.Tests/ResponseWrapperFilterTests.cs:137)
- 问题描述：生产代码对新包装的 `ObjectResult` 和 `ProblemDetails` 明确设置 `DeclaredType = null`，以便序列化实际的 `ApiResult`；仍有 8 个单元测试要求 `DeclaredType == ApiResult.Type`。这会使 CI 失败并掩盖真正的响应回归，但当前响应包装行为本身已通过生产逻辑和 ASP.NET 集成测试验证。
- 风险等级：`P1`，`工程规范｜测试回归`。
- 复现条件：执行 `dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj`。
- 根因分析：生产契约调整后只更新了部分测试（已有若干测试断言 `null`），没有统一修正所有分支断言。
- 参考修复代码：

```csharp
Assert.Null(result.DeclaredType);
Assert.IsType<ApiResult>(result.Value);
Assert.Equal(expectedJson, JsonSerializer.Serialize(result.Value));
```

- 方案取舍：按当前生产设计更新测试最小且正确；若确实要求固定声明类型，应先确认 ASP.NET formatter 对声明类型和运行时类型的行为，再统一修改生产代码和全部集成测试，不能只改断言。

## 边界case遗漏

### 31. SessionSnapshot 丢失所有设备 Header 字段

- 位置：[SessionSnapshot.cs:19-34](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/SessionSnapshot.cs:19)、[SessionSnapshot.cs:76-84](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/SessionSnapshot.cs:76)、[SessionField.cs:19-65](/Users/lewis/github/MSFramework/src/MSFramework/Application/SessionField.cs:19)
- 问题描述：快照已复制 TraceIdentifier、用户标量、Roles 和 Subjects，但 `GetValue(SessionField)` 恒返回 `null`。本地事件后台 Handler 读取 DeviceId、IMEI、经纬度、平台等字段时，会与发布请求中的会话不一致。
- 风险等级：`P2`，`边界case遗漏｜逻辑缺陷`。
- 复现条件：HTTP 请求设置 `z-device-id` 或 `z-latitude`，发布本地事件；后台 Handler 从注入的 `ISession` 读取对应字段。
- 根因分析：快照为避免捕获 Scoped 会话只复制了标量属性，却没有把按 Header 延迟读取的字段值一起物化；后台执行时原 HttpContext 已不存在。
- 参考修复代码：

```csharp
private readonly Dictionary<SessionField, string> _fields;

public SessionSnapshot(ISession session)
{
    _fields = SessionFields.All.ToDictionary(
        field => field, field => session?.GetValue(field));
    // 同时复制现有的 TraceIdentifier、身份和角色集合。
}

public string GetValue(SessionField field)
    => _fields.TryGetValue(field, out var value) ? value : null;
```

- 方案取舍：入队时物化全部已知字段最可靠，代价是每个事件会多拷贝少量字符串；只复制调用方声明使用的字段可降低开销，但要求发布 API 显式携带字段清单，复杂度更高。

### 32. 上传参数、文件大小和非可寻址输入流的边界契约不完整

- 位置：[FormFileExtensions.cs:73-132](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:73)
- 问题描述：`formFile` 有 null 校验，但 `interval` 没有 null/空白校验；`FileName` 可能为空，扩展名可能为空或包含异常长度；方法没有显式检查 `Length` 和读取取消令牌。当前对非可寻址流重新打开 `IFormFile` 的处理比旧版本更稳健，但仍假设每次 `OpenReadStream()` 都返回相同且可重复读取的内容。
- 风险等级：`P1/P2`，`边界case遗漏｜安全风险｜资源管理`。
- 复现条件：传入 `interval: null`、空文件、无扩展名文件、超大文件或自定义一次性流；客户端上传过程中取消请求。
- 根因分析：扩展方法同时承担路径构造、去重、存储和输入校验，但输入契约没有集中定义；IFormFile 的重复打开能力被当作普遍保证。
- 参考修复代码：

```csharp
ArgumentNullException.ThrowIfNull(formFile);
ArgumentException.ThrowIfNullOrWhiteSpace(interval);
if (formFile.Length is <= 0 or > MaxFileLength)
{
    throw new InvalidDataException("Invalid file length");
}

await using var input = formFile.OpenReadStream();
await CopyWithCancellationAsync(input, output, cancellationToken);
```

- 方案取舍：在 API 层用 `RequestSizeLimit`/`FormOptions` 做第一层限制，在存储 helper 做第二层限制；若输入流不可重复，应改为一次读取到受限临时文件并同时计算 hash，避免先消费后重新打开的不确定性。

### 33. JSON fieldType 强制转换和字典顺序可能导致运行时异常/误更新

- 位置：[JsonPropertyBuilderExtensions.cs:36-57](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs:36)、[JsonPropertyBuilderExtensions.cs:80-94](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs:80)
- 问题描述：反序列化后使用 `(TProperty)result` 强制转换，`fieldType` 若不是 `TProperty` 或其实现类型会在查询/快照阶段抛 `InvalidCastException`。比较器按 JSON 文本比较，字典键的插入顺序、序列化大小写/缩进或全局 serializer 变更会把语义相同对象判为 Modified。
- 风险等级：`P1/P2`，`边界case遗漏｜逻辑缺陷｜性能隐患`。
- 复现条件：错误传入一个不兼容的 concrete `fieldType`；使用 `IReadOnlyDictionary` 或普通 Dictionary 以不同顺序构造相同内容；应用运行中切换 `Defaults.JsonSerializer`。
- 根因分析：API 只在非空时检查 `fieldType`，没有验证类型可赋值性；文本序列化格式承担了结构等价判断。
- 参考修复代码：

```csharp
if (!typeof(TProperty).IsAssignableFrom(fieldType))
{
    throw new ArgumentException(
        $"{fieldType} is not assignable to {typeof(TProperty)}", nameof(fieldType));
}

var leftNode = JsonNode.Parse(CanonicalJson.Serialize(left));
var rightNode = JsonNode.Parse(CanonicalJson.Serialize(right));
return JsonNode.DeepEquals(leftNode, rightNode);
```

- 方案取舍：构建期拒绝不兼容类型能快速失败；结构化/规范化比较可消除格式噪声但仍有 CPU 成本。对不可变值对象使用原生 equality 是最优方案。

## 已关闭/基本关闭

以下旧问题经本轮源码核对已关闭，后续只需保留回归测试，不应继续按原结论报告：

| 状态 | 位置 | 本轮结论 |
| --- | --- | --- |
| 已关闭 | `src/MSFramework.Ef/DbContextBase.cs` | 审计空值/空白值差异处理已不再用 `IsNullOrWhiteSpace` 混淆变化；`columnType` 判断也已 null-safe。 |
| 已调整（有意变更，勿按旧结论报告） | `src/MSFramework.Ef/Internal/DateTimeOffsetToLongConverter.cs` | Unix 秒存储保持绝对时刻不变；2026-10 起反向转换有意改为 `.ToLocalTime()`：读回值统一携带服务器本地时区偏移，用于展示与业务判断。前提是所有实例部署时固定相同时区（容器需显式设置 TZ）。属设计决策，不再按"读回强制本地时区"报告为缺陷。 |
| 已关闭 | `src/MSFramework.Ef/Extensions/SoftDeleteQueryExtensions.cs` | 当前使用命名过滤器 `SoftDelete`，不会直接覆盖其他命名过滤器。仍需租户过滤器集成测试。 |
| 已关闭 | `src/MSFramework.Ef.Design/MSFramework.Ef.Design.csproj` | EF Core Design 依赖版本已对齐到 10.0.10。 |
| 已关闭 | `src/MSFramework/LocalEvent/ServiceCollectionExtensions.cs` | 本地事件扫描已过滤抽象类、非 class 和开放泛型 Handler。 |
| 基本关闭 | `src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs` | 正常/异常路径会恢复 Body 位置，reader 使用 `leaveOpen`；剩余问题只有大小限制，见第 24 项。 |
| 已关闭 | `src/MSFramework.AspNetCore/HttpSession.cs` | `Load(null)` 已使用 `ArgumentNullException.ThrowIfNull`，Header cache 访问有锁。 |
| 已关闭 | `src/MSFramework.AspNetCore/Mvc/ModelBinding/EnumerationModelBinder.cs` | 必填和可选空值已按 `ModelMetadata.IsRequired` 区分。 |
| 已关闭 | `src/MSFramework/Text/Json/ServiceCollectionExtensions.cs` | 默认 System.Text.Json 注册语义已返回 `IJsonSerializer`，旧的 concrete-only 注册问题不再成立。 |
| 已关闭 | `src/MSFramework.Analyzers/RepositoryInterfaceGenerator.cs`、`src/MSFramework.Ef.Analyzers/RepositoryGenerator.cs` | 生成器已过滤 abstract、internal、generic、nested 等非法类型。 |
| 已关闭 | `src/MSFramework.Ef/EfUnitOfWork.cs` | `SavedChanges` 当前使用 `GetInvocationList()` 逐个 await，旧的多播异步只等待最后任务问题不再成立；接口兼容问题仍见第 8 项。 |
| 已关闭 | `src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs` | Model `ValueComparer` 已实际设置；当前剩余的是序列化成本、格式等价性和类型契约问题，见第 14、32 项。 |

## 建议补充的测试用例

### 发布前必须补充

- Dapr：缺失 token、错误 token、伪造 `X-Forwarded-For`、合法可信代理链、endpoint 缺失、非 Topic endpoint，以及 middleware 位于 routing 前后的拒绝行为。
- 领域事件：Handler 抛异常后事件仍可重试；数据库保存失败后事件不丢失；只添加事件但没有实体状态变化；相同值的两个事件分别投递；Handler 新增事件的生命周期；Outbox 与业务变更同事务提交/回滚。
- 多 DbContext：第一个上下文成功、第二个失败时验证回滚；同库共享事务；跨库明确走 Outbox/Saga 的补偿路径。
- MySQL migration：普通表外键必须存在；外部实体表按预期排除；两个 DbContext 并行生成迁移不互相影响；验证不存在静态全局 FK 开关污染。
- 上传：并发相同内容上传、读写同时发生、进程中断、符号链接失败降级、目标已存在、不同节点共享存储、null interval、超大文件、空文件、恶意内容和允许扩展名中的 HTML 内容。
- 加密：ECB 旧数据兼容读取、AES-GCM 新格式 nonce/tag 校验、密文篡改、错误密码、低熵密码、随机 nonce 不重复，以及跨版本格式迁移。
- 审计/日志：QueryString 中 token/password 脱敏、实体敏感字段 allowlist、值长度上限、Loki 异步 sink 在 Host 停止时 flush、logger dispose 后句柄和后台线程已退出。

### 近期应补充

- EF：Owned/Keyless 类型不被属性约定修改；ObjectId/Enumeration converter 和显式列配置保持不变；JSON 可变对象、无序字典、null、错误 fieldType、固定 serializer comparer 行为。
- 仓储/分页：默认查询不加载未请求导航；删除只查询必要列；limit 为 0、负数、`int.MaxValue`、页码极值；无排序查询被拒绝；并发写入下分页稳定；Count 与 data 的隔离级别。
- 本地事件：Handler 临时失败重试/死信；停止时排空；进程崩溃恢复；SessionSnapshot 对每个 `SessionField` 的值在后台仍可读。
- API 兼容：AutoMapper 旧的 `params Assembly[]` 调用、带 configAction 的新调用、SavedChanges 旧/新订阅方式；模板项目和示例项目完整编译。
- 序列化/请求体：Newtonsoft 流重载可用且不关闭调用方流；body 超限拒绝并恢复位置；DateTime/DateTimeOffset 在 UTC、非 UTC、Unspecified、不同 Culture、空字符串和非法数字下的统一契约。
- 构建：默认 `dotnet build` 不隐式 pack；显式 `dotnet pack` 的包内容、输出目录和并行构建稳定性。
