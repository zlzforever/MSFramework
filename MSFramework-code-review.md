# MSFramework 第二轮深度代码评审

## 复审范围与结论

复审日期：2026-08-25。

范围仅包含 src/ 下目录名以 MSFramework 开头的项目，排除 Sample、测试项目和 DotNetCore.CAP.Dapr。本轮重点检查工作树中用户刚提交的修复，并回查上一版问题是否真正关闭。

结论：上一版 35 项问题中，审计空值/空白值处理、数据库 Unix 时间转换、EF Design 包版本、抽象/开放泛型本地事件 Handler 扫描等问题已修复或明显改善；但 Dapr 鉴权、领域事件事务语义、多 DbContext 部分提交、文件上传原子性、日志敏感信息、默认 DI 注册等高风险问题仍未解决。另外发现 SavedChanges 异步多播等待和领域事件重入两个新的回归风险。

验证结果：

- MSFramework.Tests：229 passed，1 skipped。
- MSFramework.AspNetCore.Test：50 passed，5 failed。
- 失败的 5 个测试均为 FormFileTests，原因是生产代码将私有字段 _existingDirCache 重命名为 ExistingDirCache，测试通过反射找不到字段。
- MSFramework.Ef、MSFramework.AspNetCore、MSFramework.Ef.Analyzers、MSFramework.Ef.Design 单项目 Release 构建成功，使用了 -p:GeneratePackageOnBuild=false。
- 解决方案级构建两次均运行约 5 分钟后失败，分别报告 0 errors/2 warnings 和 0 errors/0 warnings；未得到新的编译错误。
- 测试构建有 NU1903：SQLitePCLRaw.lib.e_sqlite3 2.1.11 存在已知高危漏洞；另有 ASP.NET 弃用警告。
- OCR CLI 的 preview 能识别 17 个产品文件，但 ocr llm test 因 DNS 无法访问 api.deepseek.com，未得到外部模型意见。以下判断基于本地源码、diff、构建和测试。

风险等级：P0 表示发布前必须修复；P1 表示近期必须修复；P2 表示应纳入后续版本。

## 已关闭或基本关闭的问题

| 状态 | 位置 | 结论 |
| --- | --- | --- |
| 已修复 | src/MSFramework.Ef/DbContextBase.cs | 审计不再用 IsNullOrWhiteSpace 混淆空值变化；仍需补充 JSON 可变对象测试。 |
| 已修复 | src/MSFramework.Ef/DbContextBase.cs | columnType 判断已改为 null-safe。 |
| 已修复 | src/MSFramework.Ef/Internal/DateTimeOffsetToLongConverter.cs | 数据库 DateTimeOffset 读回不再强制使用服务器本地时区。 |
| 已修复 | src/MSFramework.Ef/Extensions/SoftDeleteQueryExtensions.cs | 当前 EF Core 10 使用命名过滤器 SoftDelete，不再直接覆盖其他命名过滤器；必须补租户过滤器回归测试。 |
| 已修复 | src/MSFramework.Ef.Design/MSFramework.Ef.Design.csproj | EF Core 版本已与核心 EF 包对齐到 10.0.10。 |
| 已修复 | src/MSFramework/LocalEvent/ServiceCollectionExtensions.cs | 扫描已排除抽象类、非 class 和开放泛型 Handler。 |
| 基本修复 | src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs | 正常路径已恢复请求体原位置并使用 leaveOpen；异常路径和大小限制仍见下文。 |
| 基本修复 | src/MSFramework.AspNetCore/HttpSession.cs | Header 字典初始化/访问已加锁，Load 会清理缓存；Load(null) 仍未校验。 |

## 严重 Bug

### 1. Dapr Topic 鉴权仍然是 fail-open

标签：P0｜严重 bug｜安全风险

位置：[DaprSecurityMiddleware.cs:30](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs:30)、[DaprSecurityMiddleware.cs:48](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs:48)、[HttpContextExtensions.cs:32](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/HttpContextExtensions.cs:32)

- 问题：代码仍没有读取或校验注释中声明的 DAPR_API_TOKEN；Endpoint == null 直接放行；无条件信任请求头 X-Forwarded-For。
- 复现条件：公网请求发送 X-Forwarded-For: 127.0.0.1；或将中间件放在 UseRouting 前，使 Topic 请求没有 Endpoint。
- 根因：鉴权依赖可伪造的 Header，且路由元数据缺失时采取放行策略。
- 修复片段：

~~~csharp
app.UseForwardedHeaders(); // 只配置 KnownProxies/KnownNetworks
app.UseRouting();
app.UseMiddleware<DaprSecurityMiddleware>();

var expected = configuration["DAPR_API_TOKEN"];
var supplied = context.Request.Headers["dapr-api-token"].ToString();
var valid = !string.IsNullOrEmpty(expected) &&
    CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expected),
        Encoding.UTF8.GetBytes(supplied));

if (isTopic && (endpoint == null || !valid))
{
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    return;
}
~~~

- 方案取舍：Token 应作为主认证，可信代理解析后的 IP 只能作为额外限制；仅依赖 IP 不能抵抗代理配置错误或 Header 伪造。

### 2. 领域事件未建立“发布中”状态，Handler 内保存会递归发布

标签：P0｜严重 bug｜逻辑缺陷｜并发安全

位置：[DbContextBase.cs:180](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:180)、[DbContextBase.cs:189](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:189)、[DbContextBase.cs:413](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:413)

- 问题：本轮把清理动作从发布前移到保存后，但领域事件在 Handler 执行期间仍留在实体集合中。
- 复现条件：领域事件 Handler 内调用同一个 DbContext.SaveChanges 或 IUnitOfWork.SaveChangesAsync。内层保存再次看到同一个事件并重新发布，可能无限递归或重复产生外部副作用。
- 根因：事件只是读取快照，没有原子地从 Pending 状态转为 Dispatching 状态；“保存失败可重试”和“嵌套保存不重复派发”没有同时建模。
- 修复片段：推荐使用同事务 Outbox；最低限度也必须对事件做原子 dequeue，并在失败时 requeue：

~~~csharp
var pending = aggregate.DequeueDomainEvents(); // 从 Pending 移到 Dispatching
foreach (var @event in pending)
{
    AddOutboxMessage(@event);
}

await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
aggregate.AcknowledgeDomainEvents(pending);
~~~

- 方案取舍：Outbox 可保证数据库提交与事件记录在同一事务内，可靠性最高；单纯增加 _isPublishing 标志实现简单，但进程崩溃和跨进程投递仍需补偿机制。

### 3. 领域事件快照和清理边界仍会丢事件或重复事件

标签：P1｜逻辑缺陷｜边界 case

位置：[DbContextBase.cs:197](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:197)、[DbContextBase.cs:204](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:204)、[DbContextBase.cs:423](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:423)、[DbContextBase.cs:438](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextBase.cs:438)

- 问题：
  - ApplyConcepts() 返回 false 时直接返回，事件已经发布但不会清理，下一次保存会再次发布。
  - mediator == null 时不会调用 GetDomainEvents，但成功保存后仍会清空所有实体事件，事件被静默丢弃。
  - Handler 在发布过程中新增的事件会被最终的 ClearDomainEvents() 一并清除。
  - HashSet<DomainEvent> 按 record 值相等性去重，两个独立发生但字段相同的事件只发布一次。
- 复现条件：只添加领域事件而不改变实体状态；未注册 mediator；Handler 新增事件；同一聚合连续产生两个相同值的 record 事件。
- 根因：清理范围没有绑定到“本次已成功确认的具体事件”，且事件被当作值集合而非事件发生序列。
- 修复片段：

~~~csharp
var dispatched = SnapshotDomainEventsInOrder(); // List，不使用 HashSet
try
{
    await DispatchAsync(dispatched, cancellationToken);
    await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    RemoveExactEvents(dispatched);
}
catch
{
    // 保留或重新放回 dispatched，供重试
    throw;
}
~~~

- 方案取舍：按事件实例确认最精确；Outbox 则把确认点放在数据库事务中，适合跨进程集成事件。

### 4. 多 DbContext 工作单元仍可能部分提交

标签：P0｜严重 bug｜逻辑缺陷｜资源一致性

位置：[EfUnitOfWork.cs:28](/Users/lewis/github/MSFramework/src/MSFramework.Ef/EfUnitOfWork.cs:28)

- 问题：多个上下文仍按顺序独立执行 SaveChangesAsync。
- 复现条件：第一个上下文提交成功，第二个上下文因约束、网络错误或超时失败。
- 根因：UoW 没有共享事务；每个 DbContext 默认拥有独立事务。
- 修复片段（仅适用于同一数据库和同一连接）：

~~~csharp
await using var tx = await first.Database
    .BeginTransactionAsync(cancellationToken);

foreach (var context in contexts)
{
    context.Database.UseTransaction(tx.GetDbTransaction());
    await context.SaveChangesAsync(cancellationToken);
}

await tx.CommitAsync(cancellationToken);
~~~

- 方案取舍：同库可共享连接/事务；跨库不能假设本地事务原子，应使用 Outbox、Saga 或补偿事务。

## 逻辑缺陷与并发安全

### 5. SavedChanges 改为异步委托后，只有最后一个返回的任务被等待

标签：P1｜并发安全｜异常处理｜新回归

位置：[IUnitOfWork.cs:15](/Users/lewis/github/MSFramework/src/MSFramework/Domain/IUnitOfWork.cs:15)、[EfUnitOfWork.cs:26](/Users/lewis/github/MSFramework/src/MSFramework.Ef/EfUnitOfWork.cs:26)、[EfUnitOfWork.cs:35](/Users/lewis/github/MSFramework/src/MSFramework.Ef/EfUnitOfWork.cs:35)

- 问题：多播 Func<CancellationToken, ValueTask> 调用时所有订阅者会被启动，但委托返回的 ValueTask 只来自最后一个订阅者；前面的异步回调不会被等待，异常也无法可靠传回。
- 复现条件：注册两个 async SavedChanges 订阅者，令第一个延迟或抛异常，SaveChangesAsync 会在第一个完成前返回或漏掉其异常。
- 根因：不能对带返回值的多播委托直接 Invoke 并期待获得所有异步结果。
- 修复片段：

~~~csharp
var callbacks = SavedChanges?.GetInvocationList()
    .Cast<Func<CancellationToken, ValueTask>>()
    .ToArray() ?? [];

foreach (var callback in callbacks)
{
    await callback(cancellationToken);
}
~~~

- 方案取舍：顺序等待最容易保持确定性；独立回调可并行执行，但需要 ValueTask.AsTask() 后 Task.WhenAll，并明确部分失败策略。这个公开接口还改变了原 Action 的源码/二进制契约，模板和外部消费者必须同步迁移。

### 6. 本地事件仍无重试、死信和可靠停机排空

标签：P1｜逻辑缺陷｜资源可靠性

位置：[LocalEventBackgroundService.cs:45](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:45)、[LocalEventBackgroundService.cs:129](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:129)、[LocalEventBackgroundService.cs:156](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:156)

- 问题：Handler 异常只写日志；事件从 Channel 取出后没有重试/死信；停止时使用 stopping token，积压事件可能被取消。
- 复现条件：Handler 抛异常、进程崩溃、服务停止时 Channel 尚有事件。
- 根因：消费、确认和失败转移没有分离，Channel 只是内存队列。
- 修复片段：

~~~csharp
try
{
    await HandleAsync(entry, cancellationToken);
    await AckAsync(entry, cancellationToken);
}
catch (Exception ex) when (attempt < maxRetry)
{
    await RetryQueue.WriteAsync(entry, cancellationToken);
}
catch (Exception ex)
{
    await DeadLetterStore.WriteAsync(entry, ex, CancellationToken.None);
}
~~~

停止时先完成 Writer，再使用独立 drain token 消费队列。需要进程级可靠性时应使用 Outbox 或持久队列。

### 7. 外部实体缓存命中时仍先执行工厂

标签：P2｜逻辑缺陷｜性能隐患

位置：[ExternalEntityRepository.cs:40](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/ExternalEntityRepository.cs:40)

- 问题：Load(Func<TEntity>) 先执行工厂，再按实体 ID 查缓存。
- 复现条件：工厂执行 HTTP/数据库查询或包含副作用，重复加载同一实体。
- 根因：接口没有单独接收缓存 Key。
- 修复片段：

~~~csharp
public TEntity Load(TKey key, Func<TEntity> factory)
{
    var lazy = _cache.GetOrAdd(
        key,
        _ => new Lazy<TEntity>(
            factory,
            LazyThreadSafetyMode.ExecutionAndPublication));
    return lazy.Value;
}
~~~

- 方案取舍：Lazy 可抑制并发重复执行，但会缓存工厂异常；纯函数工厂可使用更简单的缓存实现。

### 8. 领域事件/工作单元公开 API 迁移不完整

标签：P1｜工程规范｜兼容性

位置：[IUnitOfWork.cs:15](/Users/lewis/github/MSFramework/src/MSFramework/Domain/IUnitOfWork.cs:15)、[template/Content/src/Template.Application/Project/V10/DomainEventHandlers/ProjectCreatedEventHandler.cs:20](/Users/lewis/github/MSFramework/template/Content/src/Template.Application/Project/V10/DomainEventHandlers/ProjectCreatedEventHandler.cs:20)

- 问题：接口从 event Action 改为 event Func<CancellationToken, ValueTask> 后，模板仍注册返回 Task 的 lambda，使用模板生成的项目可能无法编译。
- 复现条件：执行模板或外部项目仍使用 unitOfWork.SavedChanges += () => SomeTask();。
- 根因：公共接口变更没有同步更新所有仓库内消费者，也没有提供兼容适配层。
- 修复片段：

~~~csharp
unitOfWork.SavedChanges += cancellationToken =>
    new ValueTask(daprClient.PublishEventAsync(
        "pubsub", "ProjectCreatedEvent", payload,
        cancellationToken: cancellationToken));
~~~

- 方案取舍：主版本升级并统一迁移最干净；兼容期可保留旧事件并新增 RegisterSavedChangesAsync，降低下游即时破坏风险。

## 安全风险

### 9. 上传文件仍写入 wwwroot，黑名单不足以阻断可执行内容

标签：P1｜安全风险｜边界 case

位置：[FormFileExtensions.cs:21](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:21)、[FormFileExtensions.cs:78](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:78)、[FormFileExtensions.cs:83](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:83)

- 问题：本轮增加了危险扩展名黑名单，但文件仍位于 wwwroot，且 .xhtml、.xml、.mjs 等未必被拦截；interval == null 会在 Contains 处抛 NRE。
- 复现条件：上传黑名单之外、能被浏览器解释的扩展名，随后通过静态文件中间件访问；调用 SaveAsync(null)。
- 根因：黑名单无法穷举危险格式，存储目录和公开访问目录没有隔离，也没有函数级大小/MIME/内容校验。
- 修复片段：

~~~csharp
ArgumentException.ThrowIfNullOrEmpty(interval);

var extension = Path.GetExtension(formFile.FileName)
    .ToLowerInvariant();
var allowed = new HashSet<string>(
    [".jpg", ".jpeg", ".png", ".pdf"],
    StringComparer.OrdinalIgnoreCase);

if (!allowed.Contains(extension) || formFile.Length > maxBytes)
{
    throw new InvalidDataException("不支持的文件");
}

var storageRoot = Path.Combine(AppContext.BaseDirectory, "storage");
~~~

- 方案取舍：扩展名白名单和 Web root 外存储最简单可靠；若必须支持 SVG，应清洗/转码后再提供下载，并设置 Content-Disposition: attachment 和 X-Content-Type-Options: nosniff。

### 10. 上传使用 MD5 作为去重身份

标签：P1｜安全风险｜数据完整性

位置：[FormFileExtensions.cs:94](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:94)

- 问题：攻击者可利用 MD5 碰撞使不同内容共享去重路径，造成错误去重或内容替换。
- 复现条件：提交同 MD5、不同内容的构造文件；命中同一个 oss/{md5} 链接。
- 根因：MD5 只适合非安全校验，不应作为攻击者可控文件的唯一身份。
- 修复：使用 SHA-256/SHA-512，并在路径或元数据中加入算法版本，例如 v2/sha256/...。对已有 MD5 路径做兼容读取但不再写入。

### 11. AES 默认 ECB 且没有认证

标签：P1｜安全风险

位置：[Cryptography.cs:92](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Cryptography.cs:92)、[Cryptography.cs:116](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Cryptography.cs:116)

- 问题：默认 CipherMode.ECB 泄露重复块；密文没有认证标签，篡改后无法可靠检测；UTF-8 字符串直接作为 Key 也缺少 KDF。
- 复现条件：加密包含重复块的明文，或修改 Base64 密文后解密。
- 根因：使用不适合新设计的模式和无认证的旧 API。
- 修复片段：

~~~csharp
var key = Rfc2898DeriveBytes.Pbkdf2(
    secret, salt, 100_000, HashAlgorithmName.SHA256, 32);
var nonce = RandomNumberGenerator.GetBytes(12);
var ciphertext = new byte[plain.Length];
var tag = new byte[16];

using var aes = new AesGcm(key);
aes.Encrypt(nonce, plain, ciphertext, tag, associatedData);
~~~

- 方案取舍：AES-GCM 同时提供保密性和完整性；CBC+HMAC 可兼容旧平台但实现更复杂。密文格式应带版本号，不能无提示改变旧数据语义。

### 12. 审计和异常日志仍泄露 QueryString、设备信息和实体敏感值

标签：P1｜安全风险｜隐私合规

位置：[AuditAtrribute.cs:182](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Filters/AuditAtrribute.cs:182)、[LokiAuditingStore.cs:77](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:77)、[GlobalExceptionFilter.cs:47](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Filters/GlobalExceptionFilter.cs:47)

- 问题：完整 URL/QueryString、User-Agent、IP、设备标识和实体原值/新值仍进入日志。
- 复现条件：请求 URL 携带 token、password、secret，或修改密码、身份证号等实体属性后查看 Loki/文件日志。
- 根因：没有字段 allowlist、敏感字段策略和 QueryString 脱敏。
- 修复片段：

~~~csharp
var safeQuery = RedactQuery(
    httpContext.Request.Query,
    ["token", "password", "secret", "authorization"]);

var safeProperties = auditEntity.Properties
    .Where(x => AuditFieldPolicy.IsAllowed(x.Name))
    .Select(Redact);
~~~

- 方案取舍：allowlist 比黑名单安全但需要业务维护；同时应限制日志访问、保留周期并避免把完整异常上下文写入生产日志。

## 资源管理与性能隐患

### 13. 上传文件写入仍非原子，读者可见半成品

标签：P1｜并发安全｜资源管理｜数据完整性

位置：[FormFileExtensions.cs:112](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:112)、[FormFileExtensions.cs:117](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:117)、[FormFileExtensions.cs:209](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:209)

- 问题：File.Exists + FileMode.Create + FileShare.ReadWrite 存在跨线程/跨进程竞态；符号链接失败后的 File.Copy 也不是原子操作。
- 复现条件：并发上传同内容并同时读取 OSS 路径，或进程在复制期间崩溃。
- 根因：没有临时文件、排他写入和原子 rename。
- 修复片段：

~~~csharp
var temp = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
await using (var output = new FileStream(
    temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
{
    await input.CopyToAsync(output, cancellationToken);
    await output.FlushAsync(cancellationToken);
}

try
{
    File.Move(temp, finalPath, overwrite: false);
}
catch (IOException) when (File.Exists(finalPath))
{
    File.Delete(temp);
}
~~~

只有最终文件完成后，才能创建 dedupe 索引/链接。相比进程内锁，临时文件加 rename 能覆盖多实例部署。

### 14. 上传目录缓存仍无界增长

标签：P2｜性能隐患｜资源泄漏

位置：[FormFileExtensions.cs:21](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:21)

- 问题：本轮只是将 _existingDirCache 重命名为 ExistingDirCache，静态 ConcurrentDictionary 仍永久保存每个日期/哈希目录。
- 复现条件：长期运行服务持续上传，目录数量和进程内存持续增长。
- 根因：目录存在性被当作永久缓存，没有上限、过期和失效策略。
- 修复：优先直接调用幂等的 Directory.CreateDirectory；或使用有容量上限的缓存。目录创建通常比永久缓存更可靠。

### 15. EF 仓储默认 Include 全部一级导航

标签：P1｜性能隐患

位置：[EfRepository.cs:35](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/EfRepository.cs:35)、[EfRepository.cs:80](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Repositories/EfRepository.cs:80)

- 问题：普通查询、主键查询都默认 Include 所有一级导航；多个集合会扩大 SQL、内存和跟踪开销。
- 复现条件：聚合包含大集合或多个集合时调用 Find、分页查询或普通 Store 查询。
- 根因：仓储把“完整聚合加载”设为所有场景默认行为。
- 修复片段：

~~~csharp
protected virtual IQueryable<TAggregateRoot> Store => _dbSet;

protected IQueryable<TAggregateRoot> WithDetails(
    IQueryable<TAggregateRoot> query)
{
    return query
        .Include(x => x.Items)
        .Include(x => x.Customer);
}
~~~

- 方案取舍：显式 Include/投影减少默认成本；SplitQuery 只能缓解笛卡尔积，不能解决不必要的导航加载。

### 16. 分页仍无限制、无稳定排序且 Count 与数据查询不一致

标签：P1｜性能隐患｜逻辑缺陷｜边界 case

位置：[PagedQueryExtensions.cs:35](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/PagedQueryExtensions.cs:35)、[PagedQueryExtensions.cs:41](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/PagedQueryExtensions.cs:41)

- 问题：limit 只对小于 1 的值纠正，int.MaxValue 仍可进入数据库；未排序查询分页不稳定；Count 与 ToList 分两次执行。
- 复现条件：请求传入极大页大小、查询没有 OrderBy、Count 和查询之间有并发写入。
- 根因：API 没有最大页大小、唯一稳定排序和一致性读取约束。
- 修复片段：

~~~csharp
limit = Math.Clamp(limit, 1, 100);
var pageQuery = query
    .OrderBy(x => x.CreatedAt)
    .ThenBy(x => x.Id)
    .Skip(offset)
    .Take(limit);
~~~

- 方案取舍：Offset 分页易用但深页慢；Keyset 分页性能稳定但需要游标和新的 API。强一致 total/data 需要事务隔离或接受最终一致。

### 17. Loki Logger 未释放，Async sink 可能丢尾部日志

标签：P1/P2｜资源泄漏｜可靠性

位置：[LokiAuditingStore.cs:17](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:17)、[LokiAuditingStore.cs:50](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:50)、[LokiAuditingStore.cs:68](/Users/lewis/github/MSFramework/src/MSFramework.Auditing.Loki/LokiAuditingStore.cs:68)

- 问题：CreateLogger() 创建的 Serilog Logger 没有由 Store 持有并 Dispose；AddAsync 入队后立即返回。
- 复现条件：Host 停止、Provider 释放或进程快速退出前写入审计日志。
- 根因：Store 没有实现 IDisposable，也没有明确 flush 生命周期。
- 修复片段：

~~~csharp
public sealed class LokiAuditingStore :
    IAuditingStore, IDisposable
{
    private readonly Serilog.Core.Logger _logger;

    public void Dispose() => _logger.Dispose();
}
~~~

DI 应拥有 Store 的生命周期。若要求调用方等待远端确认，应将 AddAsync 改为真正的异步 sink 写入，而不是只返回 CompletedTask。

### 18. JSON 可变对象仍未配置 Model ValueComparer

标签：P1/P2｜逻辑缺陷｜性能

位置：[JsonPropertyBuilderExtensions.cs:29](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs:29)、[JsonPropertyBuilderExtensions.cs:61](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs:61)

- 问题：只配置 Provider comparer，没有为 List/Dictionary 等内存对象配置深拷贝快照；原地修改可能不被 EF 识别。
- 复现条件：查询实体后执行 entity.Metadata.Tags.Add(...)，不替换属性引用，随后 SaveChanges。
- 根因：默认快照/比较可能基于引用，而不是 JSON 结构。
- 修复片段：

~~~csharp
var comparer = new ValueComparer<TProperty>(
    (a, b) => JsonEquals(a, b),
    value => JsonHash(value),
    value => DeepClone(value));
propertyBuilder.Metadata.SetValueComparer(comparer);
~~~

- 方案取舍：深拷贝/序列化会增加 CPU 和内存；要求不可变对象整体替换性能更好，但需要改变使用约定。

## 其他边界与工程规范

### 19. Newtonsoft 流序列化接口仍未实现，并关闭调用方输入流

标签：P1｜严重 bug｜资源管理

位置：[NewtonsoftJsonSerializer.cs:64](/Users/lewis/github/MSFramework/src/MSFramework.Serialization.Newtonsoft/NewtonsoftJsonSerializer.cs:64)、[NewtonsoftJsonSerializer.cs:86](/Users/lewis/github/MSFramework/src/MSFramework.Serialization.Newtonsoft/NewtonsoftJsonSerializer.cs:86)

- 问题：流序列化仍抛 NotImplementedException；new StreamReader(json) 默认关闭传入流。
- 复现条件：调用统一的 Serialize(Stream, TValue)；反序列化后继续使用调用方流。
- 根因：接口实现不完整，资源所有权契约不清晰。
- 修复片段：

~~~csharp
using var writer = new StreamWriter(
    utf8Json, Encoding.UTF8, 1024, leaveOpen: true);
JsonSerializer.Create(_settings).Serialize(writer, value);
writer.Flush();

using var reader = new StreamReader(
    json, Encoding.UTF8, true, 1024, leaveOpen: true);
return JsonSerializer.Create(_settings).Deserialize<T>(reader);
~~~

### 20. System.Text.Json 默认注册的服务类型仍错误

标签：P1｜严重 bug｜DI

位置：[ServiceCollectionExtensions.cs:22](/Users/lewis/github/MSFramework/src/MSFramework/Text/Json/ServiceCollectionExtensions.cs:22)

- 问题：options == null 分支注册的是 TextJsonSerializer，不是 IJsonSerializer。
- 复现条件：默认调用 UseTextJsonSerializer() 后执行 GetRequiredService<IJsonSerializer>()。
- 根因：工厂注册没有显式声明服务类型。
- 修复片段：

~~~csharp
builder.Services.TryAddSingleton<IJsonSerializer>(provider =>
{
    var options = provider.GetService<JsonSerializerOptions>();
    return options == null
        ? TextJsonSerializer.Create()
        : new TextJsonSerializer(options);
});
~~~

### 21. 编译模型功能被整文件注释，形成新的公开 API 回归

标签：P1｜工程规范｜兼容性｜新回归

位置：[DbContextOptionsBuilderExtensions.cs:1](/Users/lewis/github/MSFramework/src/MSFramework.Ef/Extensions/DbContextOptionsBuilderExtensions.cs:1)、[DbContextSettings.cs:127](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DbContextSettings.cs:127)

- 问题：为绕过 ConcurrentDictionary 不能存 null 的问题，LoadModel 扩展被整文件注释，原有消费者无法再编译；UseCompiledModel 配置仍公开存在，但没有对应加载入口。
- 复现条件：调用 optionsBuilder.LoadModel("...Model, Assembly") 的应用升级到当前版本。
- 根因：用删除功能替代修复错误分支，未处理 API 兼容和配置一致性。
- 修复方案：
  - 继续支持编译模型：保留公开 API，解析失败时直接抛出带类型信息的 NotSupportedException，并使用 EF10 正式元数据 API。
  - 放弃支持：删除 UseCompiledModel、相关文档和示例，并在主版本升级说明中明确 breaking change。

### 22. 请求体读取的异常路径和大小限制仍不完整

标签：P2｜资源管理｜边界 case

位置：[HttpContextAccessorExtensions.cs:34](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs:34)、[HttpContextAccessorExtensions.cs:40](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs:40)

- 问题：正常路径已保存/恢复 Position，但读取抛异常或取消时不会恢复原位置；ReadToEndAsync 仍无函数级大小限制。
- 复现条件：请求体流读取中断、客户端断开、发送超大 Body。
- 根因：位置恢复不在 finally 中，读取策略无上限。
- 修复片段：

~~~csharp
var original = request.Body.Position;
try
{
    request.Body.Position = 0;
    return await ReadAtMostAsync(request.Body, maxBytes, cancellationToken);
}
finally
{
    request.Body.Position = original;
}
~~~

### 23. SessionSnapshot 仍丢失所有请求头设备字段

标签：P2｜逻辑缺陷｜边界 case

位置：[SessionSnapshot.cs:26](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/SessionSnapshot.cs:26)、[SessionSnapshot.cs:81](/Users/lewis/github/MSFramework/src/MSFramework/LocalEvent/SessionSnapshot.cs:81)

- 问题：后台事件的 GetValue 恒返回 null，设备 ID、IMEI、经纬度、平台等审计字段丢失。
- 复现条件：HTTP 请求设置设备 Header，发布本地事件，在后台 Handler 中读取 ISession.GetValue。
- 根因：快照只复制标量字段，没有复制请求头字段。
- 修复片段：

~~~csharp
private readonly IReadOnlyDictionary<SessionField, string> _values;

_values = Enum.GetValues<SessionField>()
    .ToDictionary(field => field, session.GetValue);

public string GetValue(SessionField field) =>
    _values.TryGetValue(field, out var value) ? value : null;
~~~

只复制必要字段，避免长期持有原始 Scoped Session。

### 24. HttpSession.Load(null) 仍然抛 NRE

标签：P2｜边界 case｜并发安全

位置：[HttpSession.cs:193](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/HttpSession.cs:193)

- 问题：虽然 Header 缓存已加锁并在 Load 后清空，但 session 没有空值校验；并发 Load 与 GetValue 时标量字段更新和缓存替换也不是一个原子快照。
- 复现条件：调用 Load(null)；同一 Session 实例被多个异步流程同时 Load/GetValue。
- 根因：参数校验缺失，状态分散在多个字段和字典中。
- 修复片段：

~~~csharp
ArgumentNullException.ThrowIfNull(session);
lock (_fieldsLock)
{
    TraceIdentifier = session.TraceIdentifier;
    // 复制其他字段
    _fields = new Dictionary<SessionField, string>();
}
~~~

更稳妥的方案是构造不可变 SessionSnapshot 后整体替换引用，减少锁粒度。

### 25. Enumeration 空输入判断仍没有使用“是否必填”语义

标签：P2｜逻辑缺陷｜边界 case

位置：[EnumerationModelBinder.cs:22](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Mvc/ModelBinding/EnumerationModelBinder.cs:22)

- 问题：Enumeration 是引用类型，IsReferenceOrNullableType 对所有 Enumeration 子类都为 true；因此即使参数/属性是必填，Binder 也会返回成功的 null，是否失败取决于后续验证器。
- 复现条件：直接调用 Binder，或使用没有 Required 且非 nullable 注解的 Enumeration 参数提交空值。
- 根因：把“引用类型”误当作“业务上可空”。
- 修复片段：

~~~csharp
if (string.IsNullOrWhiteSpace(value))
{
    if (bindingContext.ModelMetadata.IsRequired)
    {
        bindingContext.ModelState.AddModelError(
            bindingContext.ModelName, "枚举值不能为空");
        bindingContext.Result = ModelBindingResult.Failed();
    }
    else
    {
        bindingContext.Result = ModelBindingResult.Success(null);
    }
    return Task.CompletedTask;
}
~~~

### 26. JSON DateTime 转换器仍按服务器本地时区读回

标签：P2｜逻辑缺陷｜边界 case

位置：[DateTimeJsonConverter.cs:47](/Users/lewis/github/MSFramework/src/MSFramework/Text/Json/Converters/DateTimeJsonConverter.cs:47)

- 问题：Unix 秒反序列化使用 LocalDateTime；同一 JSON 在不同时区主机上得到不同的 DateTime。
- 复现条件：在 UTC、UTC+8、UTC-5 主机上反序列化相同时间戳。
- 根因：存储/传输层混入机器本地时区。
- 修复：使用 DateTimeOffset.FromUnixTimeSeconds(v).UtcDateTime，展示层再显式转换本地时间。

### 27. 全局静态 Serializer、Logger、ServiceProvider 仍会跨宿主污染

标签：P1/P2｜并发安全｜工程规范

位置：[Defaults.cs:19](/Users/lewis/github/MSFramework/src/MSFramework/Defaults.cs:19)、[Defaults.cs:54](/Users/lewis/github/MSFramework/src/MSFramework/Defaults.cs:54)、[ServiceCollectionExtensions.cs:66](/Users/lewis/github/MSFramework/src/MSFramework/ServiceCollectionExtensions.cs:66)

- 问题：多个 Host 或并行测试会互相覆盖静态 Serializer/Logger/ServiceProvider；静态 ServiceProvider 还会持有整个根 DI 图。
- 复现条件：创建两个不同配置的 Provider，先后调用 UseMicroserviceFramework，再使用第一个 Provider 的框架组件。
- 根因：宿主级依赖被放进进程级静态状态。
- 修复：核心组件直接通过 DI 注入 IJsonSerializer、ILogger 和上下文服务；静态值只能作为无宿主 fallback，不能保存根 Provider。

### 28. Runtime 只加载一次，插件和后续扫描前缀不会生效

标签：P2｜工程规范｜边界 case

位置：[Runtime.cs:30](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Runtime.cs:30)、[Runtime.cs:42](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Runtime.cs:42)、[Runtime.cs:163](/Users/lewis/github/MSFramework/src/MSFramework/Utils/Runtime.cs:163)

- 问题：第一次 Load() 后新增前缀或运行中加入 Plugin DLL 不会刷新 FrozenSet；DefinedTypes 遇到部分类型加载失败也可能中断整个扫描。
- 复现条件：框架初始化后再添加扫描前缀；插件目录新增程序集；程序集有缺失依赖类型。
- 根因：加载缓存没有 Reload 生命周期，类型扫描没有处理 ReflectionTypeLoadException。
- 修复片段：

~~~csharp
try
{
    types.AddRange(assembly.GetTypes().Where(x => x != null));
}
catch (ReflectionTypeLoadException ex)
{
    types.AddRange(ex.Types.Where(x => x != null));
}
~~~

启动型应用应要求所有扫描配置在 Load() 前完成；插件系统应提供显式、加锁的 Reload。

### 29. Repository Source Generator 只修复了 EF 实现生成器，接口生成器仍未过滤非法类型

标签：P1/P2｜工程规范｜边界 case

位置：[RepositoryInterfaceGenerator.cs:20](/Users/lewis/github/MSFramework/src/MSFramework.Analyzers/RepositoryInterfaceGenerator.cs:20)、[RepositoryInterfaceGenerator.cs:39](/Users/lewis/github/MSFramework/src/MSFramework.Analyzers/RepositoryInterfaceGenerator.cs:39)、[RepositoryGenerator.cs:97](/Users/lewis/github/MSFramework/src/MSFramework.Ef.Analyzers/RepositoryGenerator.cs:97)

- 问题：MSFramework.Ef.Analyzers 已过滤 abstract/internal/generic/nested 类型，但 MSFramework.Analyzers.RepositoryInterfaceGenerator 仍对所有 class declaration 生成接口。
- 复现条件：定义 internal、abstract、开放泛型或嵌套聚合根。
- 根因：两个生成器没有共享同一套符号筛选规则；接口生成器只检查 ExtractKey。
- 修复片段：

~~~csharp
if (symbol is not INamedTypeSymbol
    {
        TypeKind: TypeKind.Class,
        IsAbstract: false,
        IsGenericType: false,
        ContainingType: null,
        DeclaredAccessibility: Accessibility.Public
    })
{
    return null;
}
~~~

若要支持嵌套/泛型，应使用完整符号显示名和类型参数；否则应输出诊断而不是生成潜在无效代码。

### 30. Design-time Factory 创建 Scope 后不释放

标签：P2｜资源泄漏｜工程规范

位置：[DesignTimeDbContextFactoryBase.cs:29](/Users/lewis/github/MSFramework/src/MSFramework.Ef/DesignTimeDbContextFactoryBase.cs:29)

- 问题：services.CreateScope() 返回的 Scope 没有保存或释放。
- 复现条件：长生命周期进程多次调用 CreateDbContext。
- 根因：Scope 与返回的 DbContext 没有可控的共同生命周期。
- 修复：注册并使用 IDbContextFactory<TDbContext>，或用独立 DbContextOptions 构造上下文。不能简单 using var scope 后返回，因为会提前释放 DbContext。

### 31. 自动 DI 注册可能让 concrete 类型和接口得到不同实例

标签：P2｜工程规范｜逻辑缺陷

位置：[ServiceCollectionExtensions.cs:54](/Users/lewis/github/MSFramework/src/MSFramework/Extensions/DependencyInjection/ServiceCollectionExtensions.cs:54)、[ServiceCollectionExtensions.cs:94](/Users/lewis/github/MSFramework/src/MSFramework/Extensions/DependencyInjection/ServiceCollectionExtensions.cs:94)、[TypeExtensions.cs:17](/Users/lewis/github/MSFramework/src/MSFramework/Runtime/TypeExtensions.cs:17)

- 问题：实现类型先按自身注册，第一个接口又按 implementation 注册；后续接口 alias 到第一个接口，解析 concrete 和接口可能创建不同 Scoped/Singleton 实例。多个生命周期 marker 也会静默选择 Singleton 优先。
- 复现条件：同一 Scope 同时解析 Foo 和 IFoo；类型同时实现多个生命周期 marker。
- 根因：没有统一的 concrete 实例别名，也没有生命周期冲突诊断。
- 修复片段：

~~~csharp
services.Add(new ServiceDescriptor(
    implementationType, implementationType, lifetime));

foreach (var serviceType in businessInterfaces)
{
    services.Add(new ServiceDescriptor(
        serviceType,
        provider => provider.GetRequiredService(implementationType),
        lifetime));
}
~~~

同时排除 IDisposable、IAsyncDisposable 和框架 marker，并在多个生命周期 marker 时启动失败。

### 32. 目录字段重命名导致现有测试失败

标签：P1｜工程规范｜回归

位置：[FormFileExtensions.cs:21](/Users/lewis/github/MSFramework/src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs:21)、[FormFileTests.cs:242](/Users/lewis/github/MSFramework/tests/MSFramework.AspNetCore.Test/FormFileTests.cs:242)

- 问题：生产私有字段从 _existingDirCache 改为 ExistingDirCache，测试通过反射读取旧名字，导致 5 个测试失败。
- 复现条件：运行 dotnet test tests/MSFramework.AspNetCore.Test/MSFramework.AspNetCore.Test.csproj。
- 根因：测试依赖私有字段名，且新名字不符合项目私有字段 _camelCase 规范。
- 修复：恢复字段名，或增加仅测试可见的清理接口/内部抽象，让测试不依赖反射私有实现。
- 方案取舍：恢复名称风险最低；测试可见清理接口更可维护，但会增加内部 API。

### 33. GeneratePackageOnBuild 仍会放大构建竞争

标签：P2｜工程规范｜构建稳定性

位置：[package.props:4](/Users/lewis/github/MSFramework/package.props:4)、[MSFramework.Analyzers.csproj:14](/Users/lewis/github/MSFramework/src/MSFramework.Analyzers/MSFramework.Analyzers.csproj:14)

- 问题：普通 Build/Test 仍隐式 Pack；并行项目可能写同一个 .nupkg，解决方案级构建还出现长时间无输出后失败。
- 复现条件：并行执行两个测试项目或 dotnet build -m。
- 根因：构建和打包共享输出目录，日常编译流程承担了发布副作用。
- 修复：默认关闭 GeneratePackageOnBuild，发布流水线单独执行 dotnet pack --configuration Release，必要时为每个项目指定独立包输出目录。

## 建议补充的测试用例

### 必须优先补充

- Dapr：正确/错误/缺失 Token；伪造 X-Forwarded-For；UseRouting 前后顺序；缺失 Endpoint 必须拒绝。
- 领域事件：Handler 内嵌套保存不会递归；Handler 新增事件下一次保存仍可投递；保存失败可重试；同值 record 事件投递两次；无 mediator 时事件不会静默丢失。
- UoW：两个 DbContext 第一个成功、第二个失败时验证回滚；同库共享事务；跨库 Outbox。
- SavedChanges：两个异步订阅者都完成后 SaveChangesAsync 才返回；第一个订阅者异常能被观察；取消令牌传递；旧 API/模板迁移编译测试。
- 上传：同内容并发上传并立即读取；写入中断无半成品；符号链接/复制失败；SHA-256 去重；危险扩展名和允许扩展名；interval == null；超大文件；目录缓存增长上限。

### 其他回归测试

- LocalEvent：Handler 异常重试、死信、停机 drain、进程中断、设备 Header 快照、审计序列化失败。
- JSON：Newtonsoft 流接口、输入流保持打开、默认 IJsonSerializer 注入、List/Dictionary 原地修改、跨时区 JSON 往返。
- EF：命名软删除过滤器与租户过滤器并存、编译模型 API/禁用策略、分页上限/稳定排序/并发写入、默认 Include SQL。
- Session/ModelBinder：Load(null)、并发 Load/GetValue、必填和可空 Enumeration 空输入。
- Loki：DI Dispose flush、网络失败、尾部日志落盘。
- Generator：abstract/internal/generic/nested 输入对两个 analyzer 的一致行为。
- Design-time/构建：Scope 生命周期、solution build、并行 Build/Test、包输出隔离。

