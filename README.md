# MSFramework 中文文档

[![Build Status](https://dev.azure.com/zlzforever/cerberus/_apis/build/status/zlzforever.MSFramework?branchName=master)](https://dev.azure.com/zlzforever/cerberus/_build/latest?definitionId=10&branchName=master)

> 基于 .NET 8/10 的轻量级 DDD 微服务框架

---

## 一、项目概述

MSFramework 是一个轻量级的微服务框架，专为 .NET 8/10 设计，采用领域驱动设计 (DDD) 架构模式。

### 核心特性

| 特性             | 描述                                   |
| ---------------- | -------------------------------------- |
| **DDD 架构**     | 提供聚合根、值对象、领域事件等基础类型 |
| **多数据库支持** | 支持 SQL Server、MySQL、PostgreSQL     |
| **依赖注入**     | 基于标记接口的自动服务注册             |
| **审计日志**     | 支持 EF 内存审计和 Loki 审计           |
| **事件总线**     | 支持 RabbitMQ 事件总线和 Dapr 集成     |
| **API 增强**     | Swagger 集成、统一响应包装、异常过滤   |
| **Source Generator** | 自动生成仓储接口和实现，减少样板代码 |
| **EF 增强**      | JSON 映射、Unix 时间戳、软删除过滤器   |

### 架构图

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Presentation Layer                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Ordering.API                                 │   │
│  │                    (Controllers, Middlewares)                        │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                             Application Layer                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                     Ordering.Application                             │   │
│  │              (Commands, Queries, Handlers, DTOs)                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                               Domain Layer                                   │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                       Ordering.Domain                                │   │
│  │       (Aggregate Roots, Entities, Value Objects, Domain Events)      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Infrastructure Layer                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    Ordering.Infrastructure                           │   │
│  │        (Repositories, DbContext, Entity Configurations)              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              MSFramework Core                                │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────────┐   │
│  │ MSFramework  │ │ MSFramework  │ │ MSFramework  │ │ MSFramework.Ef   │   │
│  │   (Domain)   │ │ (AspNetCore) │ │  (.Analyzers)│ │   (SQL/MySQL/PG) │   │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘

依赖流向: API → Application → Domain ← Infrastructure
                                ↑
                           MSFramework
```

---

## 二、快速开始

### 2.1 安装 NuGet 包

```bash
# 核心框架
dotnet add package MSFramework

# ASP.NET Core 集成
dotnet add package MSFramework.AspNetCore

# EF Core 集成 (选择其一或多个)
dotnet add package MSFramework.Ef.SqlServer
dotnet add package MSFramework.Ef.MySql
dotnet add package MSFramework.Ef.PostgreSql

# Source Generator (自动生成仓储代码)
dotnet add package MSFramework.Analyzers          # 领域层：生成仓储接口
dotnet add package MSFramework.Ef.Analyzers       # 基础设施层：生成仓储实现

# 其他组件
dotnet add package MSFramework.AutoMapper
dotnet add package MSFramework.Serialization.Newtonsoft
dotnet add package MSFramework.EventBus.RabbitMQ
dotnet add package MSFramework.Auditing.Loki
```

### 2.2 基础配置

在 `Program.cs` 中配置框架：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 MSFramework
builder.Services.AddMicroserviceFramework(x =>
{
    x.UseDependencyInjectionLoader();      // 自动注册服务
    x.UseAspNetCoreExtension();           // ASP.NET Core 扩展
    x.UseTextJsonSerializer();            // JSON 序列化
}, "Your.Application");                   // 程序集前缀

// 添加其他必要服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 初始化框架
app.UseMicroserviceFramework();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
```

---

## 三、核心概念

### 3.1 领域驱动设计 (DDD)

#### 聚合根 (Aggregate Root)

```csharp
// 创建聚合根
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    private readonly List<OrderItem> _items;
    
    public IReadOnlyCollection<OrderItem> Items => _items;
    
    private Order(string id) : base(id)
    {
        _items = new List<OrderItem>();
    }
    
    public static Order Create(UserInfo buyer, Address address, string description)
    {
        var order = new Order(ObjectId.GenerateNewId().ToString())
        {
            Address = address,
            Buyer = buyer,
            Description = description,
            Status = OrderStatus.Submitted
        };
        
        // 添加领域事件
        order.AddDomainEvent(new OrderStartedDomainEvent(order, buyer.Id));
        return order;
    }
}
```

#### 值对象 (Value Object)

```csharp
// 使用 record 定义值对象
public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string Country { get; init; }
    public string ZipCode { get; init; }
}
```

#### 领域事件 (Domain Event)

```csharp
// 定义领域事件
public record OrderStartedDomainEvent : DomainEvent;

// 在聚合根中触发事件
public void AddDomainEvent(DomainEvent domainEvent)
{
    // 内部实现
}
```

---

### 3.2 依赖注入 (DI)

框架提供基于标记接口的自动注册：

```csharp
// 瞬态服务
public interface ITransientDependency { }

// 作用域服务
public interface IScopeDependency { }

// 单例服务
public interface ISingletonDependency { }

// 使用示例
public class OrderService : ITransientDependency
{
    public class OrderService { }
}
```

---

### 3.3 命令与查询 (CQRS)

#### 定义命令

```csharp
// 创建订单命令
public record CreateOrderCommand(
    List<OrderItemDTO> OrderItems,
    string UserId) : Request<string>;
```

#### 命令处理器

```csharp
// 命令处理器
public class CreateOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, string>
{
    public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(...);
        await orderRepository.AddAsync(order);
        return order.Id.ToString();
    }
}
```

#### 在 Controller 中使用

```csharp
[HttpPost]
public async Task<string> Create([FromBody] CreateOrderCommand command)
{
    return await mediator.SendAsync(command);
}
```

---

### 3.4 仓储模式 (Repository)

#### 定义仓储接口

```csharp
// 领域层
public interface IProductRepository
{
    Task<Product> GetByNameAsync(string name);
}
```

#### 实现仓储

```csharp
// 基础设施层
public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository
{
    public Task<Product> GetByNameAsync(string name)
    {
        return Store.FirstOrDefaultAsync(x => x.Name == name);
    }
}
```

---

### 3.5 审计 (Auditing)

#### 配置审计

```csharp
// 在 DbContext 中启用审计
public class OrderingContext : DbContextBase
{
    public OrderingContext(DbContextOptions options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
}
```

#### 使用审计存储

```csharp
// EF 内存审计
builder.Services.AddAuditing(options =>
{
    options.Store = AuditStore.EF;
});

// 或使用 Loki
builder.Services.AddLokiAuditing(options =>
{
    options.Url = "http://localhost:3100";
});
```

---

### 3.6 多数据库支持

#### SQL Server

```csharp
builder.Services.AddDbContext<OrderingContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMicroserviceFramework()
    .UseSqlServer();
```

#### MySQL

```csharp
builder.Services.AddDbContext<OrderingContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddMicroserviceFramework()
    .UseMySql();
```

#### PostgreSQL

```csharp
builder.Services.AddDbContext<OrderingContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMicroserviceFramework()
    .UsePostgreSql();
```

---

## 四、EF Core 高级配置

### 4.1 安装 Analyzers 包

MSFramework 提供两个 Source Generator 包，可自动生成仓储接口和实现，大幅减少样板代码：

```bash
# 领域层：自动生成仓储接口
dotnet add package MSFramework.Analyzers

# 基础设施层：自动生成仓储实现
dotnet add package MSFramework.Ef.Analyzers
```

### 4.2 Analyzers 自动生成

#### 领域层 - 自动生成仓储接口

当你在领域层定义聚合根后，`MSFramework.Analyzers` 会自动生成对应的仓储接口：

```csharp
// 领域层：Ordering.Domain/AggregateRoots/Order.cs
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    // ... 聚合根实现
}

// 编译后自动生成 (在 obj 目录下):
// namespace Ordering.Domain.Repositories
// {
//     public partial interface IOrderRepository : IRepository<Order, string>, IScopeDependency { }
// }
```

#### 基础设施层 - 自动生成仓储实现

`MSFramework.Ef.Analyzers` 会扫描领域层的聚合根，自动生成 EF 仓储实现：

```csharp
// 编译后自动生成:
// namespace Ordering.Infrastructure
// {
//     public partial class OrderRepository : EfRepository<Order, string>, IOrderRepository
//     {
//         public OrderRepository(DbContextFactory context) : base(context)
//         {
//             UseQuerySplittingBehavior = true;
//         }
//     }
// }
```

**命名约定**：
- 聚合根在 `*.Domain.AggregateRoots` 命名空间
- 接口生成到 `*.Domain.Repositories` 命名空间
- 实现生成到 `*.Infrastructure` 命名空间（将 Domain 替换为 Infrastructure）

### 4.3 DbContextSettings 配置

通过 `DbContextSettings` 可以细粒度控制 EF 行为：

```csharp
// appsettings.json
{
  "DbContexts": {
    "Ordering.Infrastructure.OrderingContext": {
      "ConnectionString": "Server=localhost;Database=OrderingDb;",
      "DatabaseType": "SqlServer",
      "TablePrefix": "t_",
      "UseUnderScoreCase": true,
      "AutoMigrationEnabled": false,
      "MaxBatchSize": 100,
      "CommandTimeout": 30,
      "EnableSensitiveDataLogging": false,
      "EnableDetailedErrors": false,
      "MigrationsAssembly": "Ordering.Infrastructure",
      "QuerySplittingBehavior": "SplitQuery",
      "UseCompiledModel": false
    }
  }
}
```

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `ConnectionString` | 数据库连接字符串 | - |
| `DatabaseType` | 数据库类型 (SqlServer/MySql/PostgreSql) | - |
| `TablePrefix` | 表名前缀 | - |
| `UseUnderScoreCase` | 使用下划线命名 (snake_case) | `true` |
| `AutoMigrationEnabled` | 启用自动迁移 | `false` |
| `MaxBatchSize` | 批量提交大小 | `100` |
| `CommandTimeout` | 命令超时(秒) | `30` |
| `EnableSensitiveDataLogging` | 记录敏感数据 | `false` |
| `EnableDetailedErrors` | 详细错误信息 | `false` |
| `MigrationsAssembly` | 迁移程序集 | 当前程序集 |
| `QuerySplittingBehavior` | 查询拆分策略 | `SingleQuery` |
| `UseCompiledModel` | 使用编译模型优化启动 | `false` |

### 4.4 实体类型配置扩展

#### ConfigureAuditProperties - 审计属性配置

```csharp
public class OrderConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        // 配置所有审计属性（创建、修改、删除时间和操作人）
        builder.ConfigureAuditProperties();
        
        // 或单独配置
        builder.ConfigureCreation();      // 仅创建审计
        builder.ConfigureModification();  // 仅修改审计
        builder.ConfigureDeletion();      // 仅删除审计（软删除）
    }
}
```

#### UseUnixTime - Unix 时间戳

将 `DateTimeOffset` 属性存储为 Unix 时间戳（bigint）：

```csharp
public class OrderConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        // 默认使用秒级时间戳
        builder.Property(x => x.CreationTime).UseUnixTime();
        
        // 使用毫秒级时间戳
        builder.Property(x => x.PrecisionTime).UseUnixTime(milliseconds: true);
    }
}
```

**数据库存储**：`bigint` 类型，值为 Unix 时间戳（秒或毫秒）

#### UseJson - JSON 列映射

将复杂对象存储为 JSON 列（支持 MySQL/PostgreSQL）：

```csharp
public class OrderConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        // 简单使用 - 存储为 JSONB (PostgreSQL 推荐)
        builder.Property(x => x.Extra).UseJson();
        
        // 指定 JSON 类型 (MySQL 使用 JSON)
        builder.Property(x => x.Metadata).UseJson(JsonDataType.JSON);
        
        // 接口/抽象类型 - 需要指定具体类型
        // 属性是 IReadOnlyList<string>，但运行时是 HashSet<string>
        builder.Property(x => x.List)
            .UseJson(typeof(HashSet<string>), JsonDataType.JSON);
    }
}
```

| JsonDataType | 说明 | 推荐数据库 |
|--------------|------|------------|
| `JSONB` | 二进制 JSON，支持索引和查询 | PostgreSQL |
| `JSON` | 标准 JSON 格式 | MySQL, PostgreSQL |

#### 软删除过滤器

实现 `IDeletion` 接口的实体会自动应用软删除查询过滤器：

```csharp
// 所有查询自动过滤 IsDeleted = true 的记录
var orders = await orderRepository.GetListAsync();  // 不包含已删除数据

// 若要包含已删除数据，使用 IgnoreQueryFilters
var allOrders = await dbContext.Orders
    .IgnoreQueryFilters()
    .ToListAsync();
```

### 4.5 编译模型优化

对于大型模型，可使用编译模型加速启动：

```bash
# 生成编译模型
dotnet ef dbcontext optimize \
    -s src/Ordering.API \
    -c OrderingContext \
    -p src/Ordering.Infrastructure \
    -o CompileModels \
    -n Ordering.Infrastructure.CompileModels
```

```csharp
// 配置使用编译模型
{
  "UseCompiledModel": true
}

// Program.cs
builder.Services.AddDbContext<OrderingContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.LoadModel("Ordering.Infrastructure.CompileModels.CompiledModels+OrderingContextModel, Ordering.Infrastructure");
});
```

### 4.6 复合主键 (Composite Primary Key)

当业务标识由多个成员组成（如「订单 + 产品」唯一确定一个订单项）时，需要复合主键。MSFramework 仅支持一种复合主键建模方式（多列实体，方案 B）。

#### 多列实体（方案 B，无键仓储）

实体自身多个标量属性直接作主键（**无 Id 包装**），实现**非泛型** `IAggregateRoot`。
EF Core 10 原生支持顶层标量多列 `HasKey`；配合框架的**无键仓储** `IRepository<TAggregateRoot>`
（ABP 风格）以表达式谓词查询。

```csharp
// 1. 聚合根实现非泛型 IAggregateRoot，多个标量属性作主键
public class MultiColumnOrderItem : EntityBase, IAggregateRoot
{
    protected MultiColumnOrderItem() { }

    public static MultiColumnOrderItem Create(string orderId, string productId, string name, int quantity)
        => new() { OrderId = orderId, ProductId = productId, Name = name, Quantity = quantity };

    public string OrderId { get; private set; }    // 复合主键成员 1
    public string ProductId { get; private set; }  // 复合主键成员 2
    public string Name { get; private set; }
    public int Quantity { get; private set; }
}

// 2. EF 映射：顶层标量多列主键
public class MultiColumnOrderItemConfiguration
    : EntityTypeConfigurationBase<MultiColumnOrderItem, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<MultiColumnOrderItem> builder)
    {
        builder.HasKey(x => new { x.OrderId, x.ProductId });
        // 业务列配置...
    }
}

// 3. 仓储：源码生成器自动识别非泛型 IAggregateRoot，
//    生成无键接口 IMultiColumnOrderItemRepository : IRepository<MultiColumnOrderItem>
//    与实现 MultiColumnOrderItemRepository : EfRepository<MultiColumnOrderItem>
//    查询使用成员等值谓词：
var item = await multiColumnOrderItemRepository.FindAsync(
    x => x.OrderId == orderId && x.ProductId == productId);
```

#### TKey 标量约束

`EntityBase<TKey>` / `IRepository<TAggregateRoot, TKey>` / `EfRepository<TEntity, TKey>` 的 `TKey`
**仅允许以下标量白名单**：

- `string`
- `int`
- `long`
- `ObjectId`
- `Guid`

复合主键一律走方案 B（非泛型 `EntityBase` + 无键仓储多列模式），**不**允许以值对象/record 作为
`TKey`。该约束通过 `TKey : IEquatable<TKey>` 泛型约束 + 文档/XML 注释约定落地，
属于**软约束（文档约定）而非编译器强制**——`IEquatable<TKey>` 只能排除不具备相等语义的类型，
无法排除同样实现 `IEquatable<T>` 的 record 值对象，因此依赖代码评审保证白名单不被突破。

#### 约束与注意事项

| 约束 | 说明 |
|------|------|
| 主键成员必须为标量类型 | string/int/Guid/枚举等可映射列类型，成员数 ≥ 2 |
| 多列主键用 `HasKey` 声明 | EF Core 10 原生支持顶层标量多列主键 |
| 完整键值才能定位 | 无键仓储按成员等值谓词查询，必须提供完整主键成员 |
| 键成员值不得含 `\|` | 审计 `EntityId` 以 `\|` 拼接多键值（如 `O1\|P1`），键成员值若含 `\|` 将导致 `EntityId` 有歧义 |
| 软删除 / 乐观锁 / 审计 | 按实体整体处理，与复合键天然兼容；审计 `EntityId` 以 `\|` 拼接多键值 |
| 现有单键 API | 完全不变，方案 B 为纯新增 |

完整示例见 `src/Sample/Ordering.Domain/AggregateRoots/CompositeKey/` 与
`src/Sample/Ordering.Infrastructure/EntityConfigurations/`（含 EF 迁移 `MultiColumnOrderItemSample`）。

---

## 五、API 增强

### 5.1 统一响应包装

```csharp
// 配置响应包装
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResponseWrapperFilter>();
});
```

所有 API 响应将被包装为：

```json
{
    "success": true,
    "code": 0,
    "msg": "",
    "data": { ... }
}
```

### 5.2 全局异常处理

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
```

全局异常过滤器返回项目既有 `ApiResult`，并保留异常类型对应的 HTTP 状态码。
响应包装器不会对已有 `ApiResult` 二次包装；客户端可按 `success`、`code`、`msg` 和
`data` 字段处理错误。服务端同时通过 `X-Correlation-ID` 响应头提供排查关联标识。

| 异常 | HTTP 状态码 |
|------|-------------|
| `MicroserviceFrameworkFriendlyException` | 200 |
| `UnauthorizedAccessException` | 403 |
| 其他异常（包括 `ArgumentException`、认证/资源异常、普通 `InvalidOperationException`、EF/DI 异常） | 500 |

友好异常的响应使用异常自身的业务 `code` 和 `msg`；其他异常使用 `code=500`、
`msg="系统内部错误"`，不会向客户端泄露异常细节。未授权异常使用 `code=403`。

生产环境的 500 响应只返回通用错误详情；服务端日志会保留完整异常和 correlation id，
并通过 `X-Correlation-ID` 响应头返回该标识，客户端应记录它供排查。

### 5.3 分页扩展包迁移

`PagedQueryAsync` 的公共命名空间仍为 `MicroserviceFramework.Linq.Expression`，
但实现已迁移到 `MSFramework.Ef` 包，以保证 `MSFramework` Core 不直接依赖 EF Core。
使用 EF 分页扩展的项目需要显式安装并引用：

```bash
dotnet add package MSFramework.Ef
```

代码中的 `using MicroserviceFramework.Linq.Expression;` 无需修改。仅引用
`MSFramework` Core 的项目不再自动获得 EF 分页扩展；这不是运行时降级，需按项目边界
显式添加 `MSFramework.Ef`，或在不使用 EF 的场景采用项目自己的 `IQueryable` 分页实现。

### 5.4 Swagger 增强

```csharp
builder.Services.AddSwaggerGen(x =>
{
    x.CustomSchemaIds(type => type.FullName);  // 避免Schema冲突
    x.OperationFilter<ObjectIdOperationFilter>();  // ObjectId支持
    x.SchemaFilter<ObjectIdSchemaFilter>();
});
```

---

## 六、最佳实践

### 6.1 项目结构

```
src/
├── MSFramework/                    # 核心框架 (领域原语、工具类)
├── MSFramework.AspNetCore/         # ASP.NET Core 集成
├── MSFramework.Analyzers/          # Source Generator (仓储接口生成)
├── MSFramework.Ef/                 # EF Core 基础设施
├── MSFramework.Ef.Analyzers/       # Source Generator (仓储实现生成)
├── MSFramework.Ef.SqlServer/       # SQL Server Provider
├── MSFramework.Ef.MySql/           # MySQL Provider
├── MSFramework.Ef.PostgreSql/      # PostgreSQL Provider
├── MSFramework.AutoMapper/         # AutoMapper 集成
├── MSFramework.Serialization.Newtonsoft/  # JSON 序列化
├── MSFramework.EventBus.RabbitMQ/  # RabbitMQ 事件总线
├── MSFramework.Auditing.Loki/      # Loki 审计存储
│
├── Ordering.Domain/                # 领域层
│   ├── AggregateRoots/             # 聚合根
│   │   └── Order.cs
│   ├── Repositories/               # 仓储接口 (自动生成)
│   │   └── IOrderRepository.cs     # partial interface
│   └── Events/                     # 领域事件
│
├── Ordering.Application/           # 应用层
│   ├── Commands/                   # 命令
│   ├── Queries/                    # 查询
│   └── Handlers/                   # 处理器
│
├── Ordering.Infrastructure/        # 基础设施层
│   ├── EntityConfigurations/       # EF 实体配置
│   ├── Repositories/               # 仓储实现 (自动生成 + 扩展)
│   │   └── OrderRepository.cs      # partial class
│   └── OrderingContext.cs          # DbContext
│
└── Ordering.API/                   # API 层
    ├── Controllers/
    └── Program.cs
```

### 6.2 命名规范

| 类型     | 命名规则   | 示例                               |
| -------- | ---------- | ---------------------------------- |
| 类/接口  | PascalCase | `OrderService`, `IOrderRepository` |
| 方法     | PascalCase | `GetByIdAsync`                     |
| 私有字段 | 下划线前缀 | `_orderRepository`                 |
| 常量     | PascalCase | `MaxRetryCount`                    |

### 6.3 使用建议

1. **领域层不应依赖基础设施**
   - 领域层只能引用 MSFramework 核心
   - 仓储接口定义在领域层，实现放在基础设施层

2. **使用工厂方法创建聚合根**
   - 避免直接使用 public 构造函数
   - 通过 `Create()` 工厂方法确保对象有效性

3. **始终使用 CancellationToken**
   ```csharp
   public async Task<Order> FindAsync(string id, CancellationToken cancellationToken)
   ```

4. **使用 IObjectAssembler 进行 DTO 映射**
   ```csharp
   var orderDto = objectAssembler.To<OrderDto>(order);
   ```

---

## 七、常见问题

### Q1: 如何添加自定义配置？

```csharp
// 定义配置类
public class MyOptions
{
    public string Key { get; set; }
    public int Timeout { get; set; }
}

// 在 appsettings.json 中配置
{
    "MyOptions": {
        "Key": "value",
        "Timeout": 30
    }
}

// 使用 Options 模式
public class MyService
{
    public MyService(IOptions<MyOptions> options)
    {
        var key = options.Value.Key;
    }
}
```

### Q2: 如何添加数据库迁移？

```bash
# 添加迁移
dotnet ef migrations add Init -s src/Sample/Ordering.API -c OrderingContext -p src/Sample/Ordering.Infrastructure
dotnet ef migrations add Test -s src/Sample/Ordering.API -c OrderingContext -p src/Sample/Ordering.Infrastructure

# 生成 SQL 脚本
dotnet ef migrations script -s src/Sample/Ordering.API -c OrderingContext -p src/Sample/Ordering.Infrastructure
```

### Q3: 如何使用静态模型优化启动？

```bash
# 优化 EF Core 编译模型
dotnet ef dbcontext optimize -s src/Ordering.API \
    -c OrderingContext \
    -p src/Ordering.Infrastructure \
    -o CompileModels \
    -n Ordering.Infrastructure.CompileModels
```

### Q4: 如何集成 Dapr？

```bash
# 启动 Dapr
dapr run --app-id ordering-api \
    --app-port 5001 \
    --dapr-http-port 5101 \
    --dapr-grpc-port 5102 \
    --config ./dapr/config.yaml \
    --components-path ./dapr/components
```

### Q5: 如何处理并发冲突？

```csharp
// 实体实现 IOptimisticLock
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    public string ConcurrencyStamp { get; set; }
}

// EF Core 会自动处理并发检查
```

### Q6: 如何自定义仓储方法？

```csharp
// 1. 在领域层定义自定义接口（不会自动生成）
public interface IOrderRepository : IRepository<Order, string>
{
    Task<Order> GetByOrderNoAsync(string orderNo);
    Task<List<Order>> GetPendingOrdersAsync();
}

// 2. 在基础设施层创建 partial 类扩展
// Ordering.Infrastructure/Repositories/OrderRepository.cs
public partial class OrderRepository
{
    public async Task<Order> GetByOrderNoAsync(string orderNo)
    {
        return await Store.FirstOrDefaultAsync(x => x.OrderNo == orderNo);
    }
    
    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await Store.Where(x => x.Status == OrderStatus.Pending).ToListAsync();
    }
}
```

### Q7: 如何配置多 DbContext？

```csharp
// appsettings.json
{
  "DbContexts": {
    "Ordering.Infrastructure.OrderingContext": {
      "ConnectionString": "Server=localhost;Database=OrderingDb;",
      "DatabaseType": "SqlServer"
    },
    "Inventory.Infrastructure.InventoryContext": {
      "ConnectionString": "Server=localhost;Database=InventoryDb;",
      "DatabaseType": "PostgreSql"
    }
  }
}

// Program.cs
builder.Services.AddDbContext<OrderingContext>(options =>
    options.UseSqlServer(connectionString1, o => o.Load(settings1)));

builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(connectionString2, o => o.Load(settings2)));
```

### Q8: 如何使用枚举类型？

```csharp
// 定义枚举（继承自 Enumeration）
public class OrderStatus : Enumeration
{
    public static readonly OrderStatus Submitted = new(1, "Submitted");
    public static readonly OrderStatus Paid = new(2, "Paid");
    public static readonly OrderStatus Shipped = new(3, "Shipped");
    
    public OrderStatus(int id, string name) : base(id, name) { }
}

// 实体中使用
public class Order : DeletionAggregateRoot<string>
{
    public OrderStatus Status { get; private set; }
}

// EF 会自动配置为 varchar 类型，存储枚举的 Id 值
// 无需额外配置
```

### Q9: 如何处理值对象映射？

```csharp
// 使用 OwnsOne 映射值对象到同一张表
builder.OwnsOne(o => o.Address, x =>
{
    x.Property(y => y.City).HasMaxLength(200).IsRequired();
    x.Property(y => y.Country).HasMaxLength(50).IsRequired();
    x.Property(y => y.ZipCode).HasMaxLength(20).IsRequired();
    x.Property(y => y.Street).HasMaxLength(200).IsRequired();
});

// 或使用 UseJson 存储为 JSON 列
builder.Property(x => x.Address).UseJson();
```

---

## 八、注意事项

### ⚠️ 重要提醒

1. **不要在生产代码中使用 Console.WriteLine**
   - 使用 `ILogger` 替代

2. **避免硬编码**
   - 连接字符串、API 密钥等应使用配置或环境变量

3. **正确处理异常**
   - 不要使用空的 catch 块
   - 使用具体的异常类型

4. **注意异步模式**
   - 使用 `async Task` 而非 `async void`
   - 避免 `Task.Result` 造成死锁

5. **事务一致性**
   - 使用 `IUnitOfWork` 确保事务一致性
   - 领域事件在 SaveChanges 前触发

6. **审计日志**
   - 确保敏感信息不被记录到日志

7. **Source Generator**
   - 聚合根必须在 `*.AggregateRoots` 命名空间下
   - 仓储接口会自动生成在 `*.Repositories` 命名空间
   - 自定义仓储方法使用 `partial class` 扩展

8. **JSON 列映射**
   - PostgreSQL 推荐使用 `JSONB` 类型
   - MySQL 使用 `JSON` 类型
   - 接口类型需要显式指定具体类型

---

## 九、版本信息

| 组件    | 版本       |
| ------- | ---------- |
| .NET    | 8.0 / 10.0 |
| EF Core | 9.x        |
| xUnit   | 最新版     |

---

## 十、扩展阅读

- [EF Core 文档](https://docs.microsoft.com/en-us/ef/core/)
- [DDD 模式](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
