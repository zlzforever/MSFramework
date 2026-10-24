# MSFramework 代码审计缺陷台账

审计基线：`837b78f8a674ceaa9c668d8518bcfcb775b4b62c`。本文件保留首轮审计的 33 个稳定编号；状态仅表示本轮代码证据，不把需要人工操作或架构决策的事项标记为已关闭。

状态定义：

- **已关闭**：已有生产代码修复和针对性回归测试。
- **部分缓解**：主要风险已降低，但原问题仍有明确的剩余边界。
- **未处理**：本轮没有足以关闭该项的实现或验证。
- **人工动作**：代码无法替代的凭据轮换、数据库迁移或发布流程。

## 33 项稳定台账

| ID | 级别 | 缺陷 | 主要位置 | 状态 | 证据、影响与剩余工作 |
|---:|---|---|---|---|---|
| 1 | 严重 | Dapr Topic 鉴权 fail-open | `src/MSFramework.AspNetCore/DaprSecurityMiddleware.cs` | 已关闭 | 缺失、空白或错误 token 返回 401；无 endpoint 或非回环访问返回 403；固定时间比较。`DaprSecurityMiddlewareTests` 与 `DaprSecurityPipelineTests` 覆盖 TestServer 管道、伪造远端、正确回环和各失败分支。 |
| 2 | 严重 | 领域事件在发布前清空，失败会丢失 | `src/MSFramework.Ef/DbContextBase.cs` | 已关闭 | 数据持久化成功后才分发；发布失败保留事件供重试，全部成功后清除。`DomainEventPersistenceTests` 覆盖失败重试。 |
| 3 | 严重 | 多 DbContext 工作单元非原子提交 | `src/MSFramework.Ef/EfUnitOfWork.cs` | 已关闭 | 仅允许共享同一物理连接的上下文进入同一事务；不同连接或已有外部事务在写入前明确拒绝，并提示 outbox/saga。`UnitOfWorkAtomicityTests` 覆盖共享提交和跨库拒绝。 |
| 4 | 严重 | MySQL 迁移无条件清除所有建表外键 | `src/MSFramework.Ef.MySql/` | 已关闭 | 移除静态待清理集合和生成器全局清除逻辑，遗留扩展改为无副作用兼容入口。`MySqlMigrationSqlTests` 验证多个迁移不会串扰。 |
| 5 | 严重 | Newtonsoft 流接口未实现且关闭调用方流 | `src/MSFramework.Serialization.Newtonsoft/NewtonsoftJsonSerializer.cs` | 已关闭 | 流式序列化/反序列化已实现并保持输入输出流打开；`SerializationTests` 覆盖 UTF-8 与流所有权。 |
| 6 | 高 | 领域事件快照丢序、去重独立事件且发布早于提交 | `src/MSFramework.Ef/DbContextBase.cs` | 已关闭 | 使用有序列表保留登记顺序和重复事件；工作单元期间延迟发布到提交后。回归测试验证顺序、重复项与提交时机。 |
| 7 | 高 | 本地事件失败即丢失且停机不能排空 | `src/MSFramework/LocalEvent/LocalEventBackgroundService.cs` | 部分缓解 | 增加有限重试、逐次新 scope、死信存储和有界停机排空。关键事件因缺少持久 outbox 被明确拒绝，尚未提供框架内建的 durable outbox 实现。`LocalEventReliabilityTests` 覆盖重试、死信、排空和关键事件契约。 |
| 8 | 高 | `SavedChanges` 接口变更破坏模板和消费者 | `src/MSFramework/Domain/IUnitOfWork.cs` | 已关闭 | 恢复兼容的同步 `event Action SavedChanges`，新增 `SavedChangesAsync`；提交后按同步、异步顺序调用。样例和模板迁移到异步事件，兼容测试覆盖两种消费者。 |
| 9 | 高 | 外部实体缓存命中仍执行工厂 | `src/MSFramework.Ef/Repositories/ExternalEntityRepository.cs` | 未处理 | `Load(Func<TEntity>)` 仍先执行工厂再查缓存，昂贵工厂和副作用无法避免；应改为延迟创建并补充命中测试。 |
| 10 | 高 | Owned/Keyless 实体仍被属性级约定处理 | `src/MSFramework.Ef/Internal/ModelBuildingStrategies.cs` | 已关闭 | 约定策略入口跳过 owned 与 keyless 类型；`EfQueryAndConventionTests` 验证聚合实体的 varchar/长度规则不会污染这些模型。 |
| 11 | 高 | DateTime/DateTimeOffset 非法或未指定时区语义不一致 | `src/MSFramework/Text/Json/Converters/`、`src/MSFramework.Ef/Internal/` | 未处理 | JSON 与 EF 转换仍存在本地时区、UTC 和 `Unspecified` 语义差异；需要先确定跨时区契约，再修改以免造成存量数据漂移。 |
| 12 | 中 | EF 仓储默认 Include 所有一级导航 | `src/MSFramework.Ef/Repositories/EfRepository.cs` | 已关闭 | 默认查询不再隐式 Include 导航；由调用方显式选择加载范围。`EfQueryAndConventionTests` 验证导航未自动加载。 |
| 13 | 中 | 分页缺少最大 limit、稳定排序和一致性读取 | `src/MSFramework.Ef/Extensions/PagedQueryExtensions.cs` | 部分缓解 | 新增 `MaxPageSize=1000` 并拒绝无 `OrderBy` 的分页；测试覆盖上限与排序。Count 和页面查询仍不是统一快照，强一致场景应由调用方事务隔离保证。 |
| 14 | 中 | JSON `ValueComparer` 每次比较、Hash、快照都序列化 | `src/MSFramework.Ef/Internal/CanonicalJson.cs` | 部分缓解 | 对象键规范化、稳定哈希和数组顺序语义已统一，避免字典顺序误更新；比较与快照仍需序列化，且仍依赖可变的全局 serializer，热路径性能风险保留。 |
| 15 | 高 | 上传采用预检查和共享写入导致同目标竞争 | `src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs` | 已关闭 | 单次流式写入唯一 staging 文件，校验后原子移动到内容寻址目标；失败和取消清理 staging。`FormFileTests` 覆盖并发、重复和清理路径。 |
| 16 | 高 | `Defaults` 与 `Runtime` 全局状态被多宿主覆盖 | `src/MSFramework/Defaults.cs`、`src/MSFramework/Utils/Runtime.cs` | 未处理 | 仍有进程级可变状态；多宿主和并行测试可互相污染。应迁移到 DI options/实例级上下文。 |
| 17 | 中 | `ExistingDirCache` 无界增长 | `src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs` | 未处理 | 静态目录缓存仍不会淘汰，动态租户/日期路径会持续增长；应移除缓存或引入容量和淘汰策略。 |
| 18 | 高 | 上传写入 wwwroot 且危险扩展黑名单不足 | `src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs` | 已关闭 | 默认存储移到 Web 根目录外，改为扩展名与 MIME 的正向允许列表，并允许注入隔离的存储根。 |
| 19 | 高 | 使用 MD5 作为文件身份 | `src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs` | 已关闭 | 内容寻址升级为 SHA-256；同内容复用、扩展名与 MIME 校验由上传回归测试覆盖。 |
| 20 | 高 | AES-ECB 无认证且字符串直接作为 Key | `src/MSFramework/Utils/Cryptography.cs` | 已关闭 | 新密文采用版本化 AES-GCM，随机 salt/nonce、PBKDF2-SHA256 和认证标签；保留旧 ECB 解密兼容。`CryptographyTests` 覆盖篡改、错误口令、随机 nonce 和旧密文。 |
| 21 | 高 | 审计/异常日志记录完整 URL、QueryString 和实体原值 | `src/MSFramework.Auditing.Loki/AuditLogSanitizer.cs` | 已关闭 | URL/query/实体值统一脱敏与截断，默认敏感键覆盖 token、password、secret 等。`AuditLogSanitizerTests` 验证泄漏阻断。 |
| 22 | 中 | Loki logger 未释放且异步 sink 尾日志无确认语义 | `src/MSFramework.Auditing.Loki/LokiAuditingStore.cs` | 部分缓解 | store 实现 `IDisposable` 并释放 logger 生命周期；异步 sink 仍没有可等待的持久确认/flush 契约，强审计场景需外部可靠通道。 |
| 23 | 中 | DesignTime 工厂创建的 Scope 未释放 | `src/MSFramework.Ef/DesignTimeDbContextFactoryBase.cs` | 已关闭 | 改为依赖 `IDbContextFactory<TContext>`，不再泄漏 scope；缺失注册给出可操作异常。样例和模板注册相应 factory，测试覆盖成功和缺失路径。 |
| 24 | 中 | 请求体读取没有函数级大小上限 | `src/MSFramework.AspNetCore/Extensions/HttpContextAccessorExtensions.cs` | 已关闭 | 默认 1 MiB 上限，可显式配置并传递取消令牌；已知/未知长度超限均失败且恢复可寻址流位置。`HttpContextTests` 覆盖边界。 |
| 25 | 中 | AutoMapper 删除程序集参数重载 | `src/MSFramework.AutoMapper/ServiceCollectionExtensions.cs` | 已关闭 | 恢复 builder/service collection 的参数为空、仅配置、仅程序集、配置加程序集四类入口；兼容测试编译并解析旧的程序集调用。 |
| 26 | 中 | 编译模型 API 已注释但文档仍宣传 | `README.md`、`src/MSFramework.Ef/Internal/ModelBuildingStrategies.cs` | 未处理 | README 仍包含 `UseCompiledModel`/`LoadModel` 示例，而实现路径已注释；需决定恢复功能或删除文档后才能关闭。 |
| 27 | 中 | 审计字段长度变更缺少 migration/schema 兼容检查 | `src/MSFramework.Auditing/Model/` | 人工动作 | 模型变更不能代替已有数据库迁移；发布前需为每个数据库生成并审阅 migration，验证截断、索引和回滚策略。 |
| 28 | 中 | 默认构建隐式打包放大并行构建竞争 | 项目构建属性 | 未处理 | 常规 build 仍可能触发 pack/共享输出竞争；应将打包从默认构建移到显式 CI 发布步骤。 |
| 29 | 中 | 版本字符串不符合程序集版本格式 | `version.props` | 未处理 | `0.15.5-beta02` 仍写入 `FileVersion` 并产生警告；应拆分 SemVer 包版本与纯数字 Assembly/FileVersion。 |
| 30 | 中 | ResponseWrapper 测试契约未随生产修复同步 | `tests/MSFramework.Tests/ResponseWrapperFilterTests.cs` | 已关闭 | 测试已按当前 DeclaredType/运行时类型包装语义同步；全量测试作为交付门禁。 |
| 31 | 边界 | SessionSnapshot 丢失设备 Header 字段 | `src/MSFramework/LocalEvent/SessionSnapshot.cs` | 已关闭 | 快照物化全部 16 个会话字段、空值默认和 Refresh；`SessionSnapshotTests` 覆盖字段完整性。 |
| 32 | 边界 | 上传参数、大小及非可寻址流边界不完整 | `src/MSFramework.AspNetCore/Extensions/FormFileExtensions.cs` | 已关闭 | 校验 interval、空文件、声明长度和真实流字节数，支持取消和不可寻址输入；边界测试覆盖恰好上限、超限与异常清理。 |
| 33 | 边界 | JSON fieldType 强转和字典顺序导致异常/误更新 | `src/MSFramework.Ef/Extensions/JsonPropertyBuilderExtensions.cs` | 已关闭 | 配置时验证 `fieldType` 可赋值性，反序列化/快照异常给出上下文；规范 JSON 对象键顺序并保留数组顺序。`JsonValueComparerTests` 覆盖类型和等价顺序。 |

## 本轮安全配置

样例、模板和测试配置中的数据库、RabbitMQ、Loki 固定凭据或私网地址已改为空值/占位方式；README 给出环境变量注入键。仓库文件移除秘密并不使 Git 历史中的值失效，发布前必须轮换所有曾提交的数据库、消息队列、Loki 及其他外部服务凭据，并按需清理历史。

## 验证索引

针对性测试集中在：`DaprSecurityMiddlewareTests`、`DomainEventPersistenceTests`、`UnitOfWorkAtomicityTests`、`MySqlMigrationSqlTests`、`LocalEventReliabilityTests`、`CryptographyTests`、`AuditLogSanitizerTests`、`FormFileTests`、`HttpContextTests`、`SessionSnapshotTests`、`PagedQueryTests`、`EfQueryAndConventionTests`、`JsonValueComparerTests`、`DesignTimeDbContextFactoryTests`、`ObjectAssembler`。

完整解决方案、模板构建、依赖漏洞和秘密扫描结果以修复轮次 2 的交付记录为准。
