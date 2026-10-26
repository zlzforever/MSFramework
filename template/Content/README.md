# Template

.NET 10 / MSFramework DDD API 模板，默认 MySQL 8、JWT、官方 Dapr App API Token、软删除与 EF 审计。

## 目录与依赖

```text
src/
  Template.API/             # 组合根、业务 HTTP 与 Dapr 回调
  Template.Application/     # Products：命令、查询契约、输出、集成事件
  Template.Domain/          # 聚合、仓储接口；生成器补齐仓储基础契约
  Template.Infrastructure/  # EF 映射、查询投影、生成的仓储实现、提交后通知
tests/
  Template.Tests/           # 无外部服务的领域、JWT、CORS、绑定与宿主测试
```

依赖方向：API → Application/Infrastructure，Infrastructure → Application/Domain，Application → Domain。
Domain 不引用技术适配包。包版本集中在 `Directory.Packages.props`，框架包统一使用 `MSFrameworkVersion`。
构建产物沿用 .NET 默认 bin/obj（已忽略），发布输出使用 build/。

## 首次配置

模板不含可用数据库密码。先在本机安全配置连接与 HTTPS 身份服务：

```bash
dotnet user-secrets set "DbContexts:0:ConnectionString" "<从安全渠道取得的开发库连接>" --project src/Template.API
dotnet user-secrets set "Authentication:Authority" "https://identity.example.com" --project src/Template.API
dotnet user-secrets set "Cors:AllowedOrigins:0" "https://frontend.example.com" --project src/Template.API
```

`Authentication:Audience` 和 `Authentication:Scope` 默认为 `template-api`，需与签发服务一致。
业务接口始终校验 JWT 签名、有效期、受众和 Scope，Debug 不绕过鉴权。
部署使用 `DbContexts__0__ConnectionString`、`Authentication__Authority` 等环境变量，不提交 `.env`。
生产应用账号仅授予业务读写权限，迁移使用单独账号。

## 构建、测试与启动

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/Template.API
```

测试使用随机内存签名密钥和只读查询替身，不连接 MySQL、OIDC 或 Dapr。
`/healthcheck` 是进程存活检查，不代表数据库已就绪。
API：GET `/api/v1.0/products?page=1&limit=20`、POST `/api/v1.0/products`、DELETE `/api/v1.0/products/{id}`。
创建 JSON：`{"name":"商品","price":100,"productType":"Home"}`，价格单位为分。
响应统一包装为 ApiResult；写请求由框架工作单元自动提交，处理器不得嵌套 SaveChanges。

## 数据库迁移

MySQL 兼容版本在 `StartupExtensions` 明确配置，不通过 AutoDetect 在启动期连接数据库。
自动迁移默认关闭，生产通过部署任务执行；首次开发先生成并应用迁移：

```bash
dotnet ef migrations add Init -s src/Template.API -p src/Template.Infrastructure -c TemplateDbContext
dotnet ef database update -s src/Template.API -p src/Template.Infrastructure -c TemplateDbContext
```

框架不生成数据库外键，引用完整性由领域逻辑负责。单个工作单元仅提交主上下文。
未启用编译模型，不引用旧 Ef.Design 空壳包。

## Dapr

应用与 sidecar 必须配置相同的 `APP_API_TOKEN`，从 Secret 系统取得，不写进脚本。
sidecar 使用标准 `DAPR_HTTP_PORT` / `DAPR_GRPC_PORT`，不硬编码端口。

```bash
dapr run --app-id template-api --app-port 5000 -- dotnet run --project src/Template.API
```

自行配置名为 `pubsub` 的消息组件。`/events/products/created` 使用独立 Dapr 授权策略，JWT 不能替代 App API Token。
网关必须禁止外部访问 `/dapr/subscribe`。
商品通知保留提交后尽力发布的样例语义：失败需日志对账，Dapr 不会重试从未成功发布的消息。
不可丢失的业务应替换为同事务 Outbox；未运行 sidecar 时通知失败不翻转已成功提交。

## 容器

Dockerfile 使用 .NET 10 和非 root app 用户，HTTP 端口 8080，storage 目录可写。
生产 HTTPS 由网关终止；不要把令牌或密码烘焙进镜像。
`IMAGE_NAME` 指定发布镜像，`up.sh` 从外部 `ENV_FILE` 注入配置，不删除已有容器。
