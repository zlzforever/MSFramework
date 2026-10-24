# MSFramework 全仓深度审查与修复报告

审查日期：2026-10-25
基准 SHA：`65eb5f1407645236eb244f6a4232b5ccd04687fb`（`origin/master`）

## 范围与方法

本轮覆盖解决方案中的 20 个项目，以及 `src/`、`tests/`、`template/`、构建与包配置。审查方法包括：

- 对 `NotImplementedException`、空异常处理、同步阻塞异步、可变静态状态、转发头、安全配置和迁移生成器做全仓模式扫描；
- 沿 Dapr、EF Core、序列化和 NuGet 依赖链做源码级调用分析；
- 为能稳定复现的问题先补失败测试，再实施最小修复；
- 执行 Release 全量测试和 NuGet 直接/传递依赖漏洞扫描。

结论：本轮关闭 4 类可验证缺陷，其中包括 2 类高风险安全问题；仍有 3 项 P0 架构风险和 2 项 P1 可靠性风险，需要独立设计和迁移计划，不能用局部补丁伪装为已解决。

## 已修复问题

### FIX-01 P0：Dapr Topic 可通过伪造 X-Forwarded-For 绕过来源限制

- 位置：`src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs:25-58`、`src/MSFramework.AspNetCore/Extensions/HttpContextExtensions.cs:30-53`
- 证据：基准实现优先采用客户端可控的 `X-Forwarded-For`。外部连接 `203.0.113.10` 携带 `X-Forwarded-For: 127.0.0.1` 时会进入下游处理器。
- 修复：安全判定只读取服务器连接信息 `HttpContext.Connection.RemoteIpAddress`；只有非空回环地址可进入 Dapr topic action。通用 IP 辅助方法保持不变，避免改变审计等非安全调用方的代理语义。
- 回归：`DaprSecurityMiddlewareTests` 在修复前 1 失败/1 通过；修复后 2/2 通过，覆盖伪造头拒绝和真实回环放行。

### FIX-02 P0：解决方案包含已知高危传递依赖

- 位置：`tests/MSFramework.Tests/MSFramework.Tests.csproj`、`tests/MSFramework.AspNetCore.Test/MSFramework.AspNetCore.Test.csproj`、`src/MSFramework.Ef.SqlServer/MSFramework.Ef.SqlServer.csproj`
- 证据：基准依赖审计报告 `SQLitePCLRaw.lib.e_sqlite3 2.1.11` 的 `GHSA-2m69-gcr7-jv3q`，以及 `System.Security.Cryptography.Pkcs 9.0.4` 的 `GHSA-846f-f764-76mx` 和 `GHSA-2fmv-rv2c-59x7`。
- 修复：测试项目显式提升 `SQLitePCLRaw.bundle_e_sqlite3` 至 `3.0.5`，SQL Server provider 显式提升 `System.Security.Cryptography.Pkcs` 至 `10.0.13`。
- 验证：`dotnet list MSFramework.sln package --vulnerable --include-transitive` 对全部 20 个项目报告无已知易受攻击包。

### FIX-03 P1：CAP Dapr 异步契约运行时抛 NotImplementedException

- 位置：`src/DotNetCore.CAP.Dapr/DaprConsumerClient.cs:31-166`、`src/DotNetCore.CAP.Dapr/DaprConsumerClientFactory.cs:49-61`
- 证据：CAP 当前 `IConsumerClient`/`IConsumerClientFactory` 契约会调用这些异步成员；基准实现的订阅、监听、提交、拒绝、释放和工厂创建全部抛出占位异常。
- 修复：异步成员复用已有同步行为并返回已完成任务；异步工厂复用 `Create`。
- 回归：`DaprConsumerClientTests` 修复前 3/3 失败，修复后 3/3 通过，覆盖 topic 路由注册、拒绝响应和创建/生命周期。

### FIX-04 P1：Newtonsoft 流序列化未实现且反序列化关闭调用方流

- 位置：`src/MSFramework.Serialization.Newtonsoft/NewtonsoftJsonSerializer.cs:57-93`
- 证据：基准 `Serialize(Stream, ...)` 直接抛占位异常；`Deserialize(Stream)` 使用默认 `StreamReader` 所有权，返回时关闭输入流。
- 修复：使用 UTF-8 `StreamWriter`/`StreamReader` 与 Newtonsoft 流式 reader/writer，并统一 `leaveOpen: true`。
- 回归：`SerializationTests` 修复前分别出现占位异常和流已关闭，修复后 2/2 通过。

## 未解决风险

### RISK-01 P0：领域事件在数据库提交前发布并提前清除

- 位置：`src/MSFramework.Ef/DbContextBase.cs:177-229`、`src/MSFramework.Ef/DbContextBase.cs:413-437`
- 证据：`SaveChangesAsync` 先调用 mediator，再执行 `ApplyConcepts` 和数据库保存；`GetDomainEvents` 使用 `HashSet` 聚合事件并立即清空聚合根中的事件。Handler 或数据库失败时，事件既可能产生先行外部副作用，也无法重试；值相同的独立事件还可能被去重。
- 建议：采用事务 Outbox；若短期无法引入 Outbox，至少将事件改为有序实例队列，并在确认成功后 ack，失败时保留待处理状态。

### RISK-02 P0：多 DbContext 工作单元存在部分提交

- 位置：`src/MSFramework.Ef/EfUnitOfWork.cs:29-46`
- 证据：`SaveChangesAsync` 顺序保存每个 DbContext，没有共享事务。第二个上下文失败时，第一个上下文已经提交。
- 建议：同库同连接共享事务；跨库显式采用 Saga/Outbox 和幂等补偿，不应继续宣称原子工作单元。

### RISK-03 P0：MySQL 迁移全局清空所有建表外键

- 位置：`src/MSFramework.Ef.MySql/ServiceCollectionExtensions.cs:34-68`、`src/MSFramework.Ef.MySql/MigrationsSqlGenerator.cs:29-78`
- 证据：`Load` 无条件设置进程级静态 `RemoveForeignKey = true`；生成器随后对每个 `CreateTableOperation` 执行 `ForeignKeys.Clear()`。与此同时，`EntityTypeConfigurationBase` 已通过 `SetIsTableExcludedFromMigrations(true)` 排除外部实体，因此该全局行为既冗余又会移除普通业务表外键。
- 建议：先补真实迁移 SQL 的兼容性测试，再删除静态开关和全局清理逻辑。此项会改变既有迁移输出，应作为独立变更审查。

### RISK-04 P1：Dapr 仍缺少独立身份凭证与中间件顺序保护

- 位置：`src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs:25-58`
- 证据：本轮关闭了转发头伪造，但当前契约仍只依赖回环连接；中间件位于路由匹配前时 `Endpoint == null` 会直接放行，也没有配置应用 token 的稳定公共契约。
- 建议：定义显式、默认拒绝的 app token 配置，并在启动扩展中强制或验证 `UseRouting` 后的顺序；增加 TestServer 级端到端测试。

### RISK-05 P1：本地事件处理是 best-effort，失败和停机可丢事件

- 位置：`src/MSFramework/LocalEvent/LocalEventBackgroundService.cs:41-161`、`src/MSFramework/LocalEvent/LocalEventChannel.cs:8-17`
- 证据：事件从内存 Channel 读出后没有 ack、重试或死信；Handler 异常仅记录日志，进程退出时积压也无法恢复。
- 建议：关键事件迁移到持久化 Outbox/队列；非关键事件至少增加有限重试、死信记录、停机排空期限，并在 API 契约中声明 best-effort。

## 验证结果

- `dotnet test MSFramework.sln --configuration Release --no-restore --verbosity minimal`：299 通过、0 失败、1 跳过；其中 `MSFramework.Tests` 244 通过/1 跳过，`MSFramework.AspNetCore.Test` 55 通过。
- `dotnet list MSFramework.sln package --vulnerable --include-transitive`：20 个项目均无已知易受攻击包。
- `git diff --check`：通过。
- `dotnet format MSFramework.sln --verify-no-changes --no-restore --verbosity minimal --include <本轮改动的 C# 文件>`：通过。
- 基准构建与测试：292 通过、0 失败、1 跳过；本轮新增 7 个回归测试。

全仓 `dotnet format MSFramework.sln --verify-no-changes --no-restore` 仍会因基准中大量未触及文件的空白、字符集和 using 排序问题失败；本轮没有用大范围格式化掩盖功能修复。验证还保留以下非阻断警告：`0.15.5-beta02` 不符合四段式程序集版本格式，部分生成的 NuGet 包缺 README，ASP.NET Core 测试仍使用已弃用的 `WebHostBuilder`/旧 `TestServer` 构造方式，另有 1 个既有 xUnit analyzer 警告。这些警告未被删除或抑制。

`ocr` 自动复审因当前环境的 DeepSeek provider 未配置 API key 而无法运行；本报告结论来自源码调用链、红绿回归测试、全量测试和依赖审计，未声称获得外部模型复核。
