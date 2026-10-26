# MSFramework - Agent Guidelines

A lightweight DDD microservice framework for .NET 10, shipped as multiple NuGet packages plus a sample app. This document guides AI coding agents working in this repository.

## Build, Lint, and Test Commands

```bash
# Build the entire solution
dotnet build

# Build in Release mode (also produces NuGet packages via GeneratePackageOnBuild)
dotnet build --configuration Release

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj
dotnet test tests/MSFramework.AspNetCore.Test/MSFramework.AspNetCore.Test.csproj   # ASP.NET Core integration tests

# Run a single test class
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj --filter "FullyQualifiedName~DomainEventTests"

# Run a single test method
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj --filter "FullyQualifiedName~DomainEventTests.DispatchTo1HandlerTests"

# EF Core migrations for the sample app (from project root)
dotnet ef migrations add Init -s src/Sample/Ordering.API -c OrderingContext -p src/Sample/Ordering.Infrastructure
dotnet ef migrations script -s src/Sample/Ordering.API -c OrderingContext -p src/Sample/Ordering.Infrastructure

# Pack NuGet packages
dotnet pack --configuration Release
```

There is no separate lint/format step — `.editorconfig` is the style contract. CI (`.github/workflows/dotnet.yml`, on push/PR to `master`) only builds and runs the two test projects.

## Critical Facts (easy to get wrong)

- **Package names ≠ namespaces.** Assemblies/packages are `MSFramework.*` but root namespaces are `MicroserviceFramework.*` (e.g. package `MSFramework.Ef` → namespace `MicroserviceFramework.Ef`). Always write `using MicroserviceFramework...`.
- **One version for all packages**: defined centrally in `version.props`; every csproj imports it plus `package.props`. Never set versions per project.
- **Target frameworks**: `net10.0` everywhere, except the two source-generator projects (`MSFramework.Analyzers`, `MSFramework.Ef.Analyzers`) which target `netstandard2.0`. `LangVersion` is `latest`.
- **Own mediator, not MediatR**: commands derive from `MicroserviceFramework.Mediator`'s `Request`/`Request<TResponse>`; handlers implement `IRequestHandler<TRequest, TResponse>` with `HandleAsync` (not `Handle`).
- **Compiled models are disabled**: the `LoadModel` options extension in `MSFramework.Ef` is fully commented out and `DbContextSettings.UseCompiledModel` has no wired-up effect — do not follow the README's compiled-model/`dbcontext optimize` instructions.
- **Paging extension lives in the `MSFramework.Ef` package** but keeps the `MicroserviceFramework.Linq.Expression` namespace (Core must not depend on EF). Referencing Core alone does not provide EF paging.
- **Commit style**: Conventional Commits with Chinese subjects, e.g. `feat(框架): ...`, `fix(响应包装): ...`.
- **Publishing** uses `publish.sh` / `publish_my_packages.sh` (`dotnet build -c Release` + `dotnet pack` + `dotnet nuget push`) and requires `NUGET_SERVER`/`NUGET_KEY` env vars. `publish_template.sh` packs the `template/` project template.

## Project Structure

Framework packages under `src/`, sample code under `src/Sample/`, tests under `tests/` (unit/integration + `Benchmark`).

```
src/
├── MSFramework/                    # Core: domain primitives, mediator, local event bus, DI, utilities
│   ├── Common/                     # ApiResult, ApiResultWithErrors (moved from AspNetCore)
│   ├── Domain/                     # Aggregate roots, entities, value objects, events
│   ├── Mediator/                   # CQRS mediator (Request, IMediator, IRequestHandler)
│   └── LocalEvent/                 # In-process event bus
├── MSFramework.AspNetCore/         # Filters: response wrapping, global exceptions, UoW; Dapr security
├── MSFramework.Ef/                 # DbContextBase, EfRepository, DesignTimeDbContextFactoryBase, model conventions
├── MSFramework.Ef.SqlServer/ MySql/ PostgreSql/   # EF provider integrations
├── MSFramework.Analyzers/          # Source Generator: repo interfaces (from domain layer)
├── MSFramework.Ef.Analyzers/       # Source Generator: repo implementations (from infrastructure layer)
├── MSFramework.Ef.Design/          # Empty shell (no source); kept only for NuGet continuity
├── MSFramework.AutoMapper/         # IObjectAssembler via AutoMapper
├── MSFramework.Serialization.Newtonsoft/
├── MSFramework.Dapr/               # Dapr integration: distributed lock (IDistributedLock impl, UseDaprLock)
├── MSFramework.Auditing.Loki/      # DEPRECATED (marked [Obsolete], kept for legacy consumers)
├── DotNetCore.CAP.Dapr/            # UNMAINTAINED (not used in production; bugs not fixed)
└── Sample/
    └── Ordering.{Domain,Application,Infrastructure,API}/       # main DDD sample
    # 注：Dapr 分布式事务样例（Ordering.DistributedTransaction.API）已删除，commit 8505696
```

Per-package guides with module maps exist at `src/MSFramework/AGENTS.md`, `src/MSFramework.AspNetCore/AGENTS.md`, and `src/MSFramework.Ef/AGENTS.md` — read the relevant one before editing those projects.

Sample dependency flow: API → Application → Domain ← Infrastructure. The Domain layer must never reference infrastructure packages.

## Code Style Guidelines (from .editorconfig)

- 4-space indent for C#, 2 for XML/JSON; UTF-8, final newline, trimmed trailing whitespace.
- Allman braces; prefer `var`; `System.*` usings sorted first; constants in PascalCase.
- Primary constructors are the preferred DI style: `public class Handler(IOrderRepository orderRepository)`.
- Records for DTOs, commands, queries, and events.
- Private fields use an underscore prefix when using traditional constructors.

## Framework Patterns

### Aggregate Roots

Pick the base class by auditing needs; keep constructors private for EF and expose `Create`/`New` factory methods that also raise domain events:

```csharp
public class Entity1(int id) : CreationAggregateRoot<int>(id);           // creation audit only
public class Product : ModificationAggregateRoot { ... }                  // + modification audit (ObjectId key)
public class Order : DeletionAggregateRoot<string>, IOptimisticLock { ... } // + soft delete
```

### Domain Events & CQRS

```csharp
public record OrderStartedDomainEvent : DomainEvent;
order.AddDomainEvent(new OrderCancelledDomainEvent(this));   // inside the aggregate, dispatched on SaveChanges

public record CreateOrderCommand(List<OrderItemDTO> OrderItems, string UserId) : Request<string>;

public class CreateOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, string>
{
    public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken) { ... }
}
```

### Source Generators (repository boilerplate)

- `MSFramework.Analyzers` generates repository **interfaces** into the domain layer when the aggregate root's namespace contains an `AggregateRoots` (or `Aggregates`) segment: `*.Domain.AggregateRoots` → `*.Domain.Repositories`.
- `MSFramework.Ef.Analyzers` generates the EF **implementations**: `*.Domain.AggregateRoots` → `*.Infrastructure` (Domain replaced by Infrastructure).
- Custom repository methods go in your own `partial` declarations of the generated interface/class (see README Q6).
- Non-generic `IAggregateRoot` with multi-column `HasKey` entities generate keyless repositories (`IRepository<T>` queried by predicate). `TKey` is restricted to `string`/`int`/`long`/`Guid`/`ObjectId` (soft convention, not compiler-enforced).

### Repositories & DbContext

```csharp
public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository
{
    public Task<Product> GetByNameAsync(string name) => Store.FirstOrDefaultAsync(x => x.Name == name);
}
```

`DbContextFactory` resolves the scoped context; `Store` is the `DbSet`. Entity configurations derive from `EntityTypeConfigurationBase<TEntity, TContext>` and use `builder.ConfigureAuditProperties()`, `.UseJson()`, `.UseUnixTime()`, `.HasQueryFilterSoftDelete()` (soft delete auto-filters `IDeletion` entities). DbContext settings come from the `DbContexts` section of `appsettings.json` (`DatabaseType`, `TablePrefix`, `UseUnderScoreCase`, `QuerySplittingBehavior`, ...).

### API Controllers & Error Handling

```csharp
[Route("api/v1.0/orders")]
public class OrderController(IOrderRepository orderRepository, IMediator mediator) : ApiControllerBase
{
    [HttpPost]
    public async Task<string> Create([FromBody] CreateOrderCommand command) => await mediator.SendAsync(command);
}
```

Responses are auto-wrapped as `ApiResult` (`success`/`code`/`msg`/`data`) unless they already are one. `MicroserviceFrameworkFriendlyException` → business code + 200; `UnauthorizedAccessException` → 403; anything else → generic 500. Validate arguments with `Check.NotNull` / `Check.NotNullOrEmpty` (`MicroserviceFramework.Utils`).

### DI Bootstrap

```csharp
builder.Services.AddMicroserviceFramework(x =>
{
    x.UseDependencyInjectionLoader();   // marker interfaces: ITransient/IScope/ISingletonDependency
    x.UseAspNetCoreExtension();
}, "Your.Application");                // assembly-prefix filter for scanning
...
app.UseMicroserviceFramework();         // required at startup
```

## Testing

- xUnit, Arrange-Act-Assert. No external services required — tests bootstrap the framework in-process:

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddMicroserviceFramework(x => { x.UseDependencyInjectionLoader(); ... });
var serviceProvider = serviceCollection.BuildServiceProvider();
serviceProvider.UseMicroserviceFramework();   // required before resolving framework services
```

- Test projects access framework internals via `InternalsVisibleTo` (`MSFramework` → `MSFramework.Ef`; `MSFramework.Ef` → the three provider projects; `MSFramework.AspNetCore` → `MSFramework.AspNetCore.Test`).

## Key Dependencies

- .NET 10, EF Core 10.x, `Microsoft.Extensions.*` 10.x
- `MongoDB.Bson` (provides `ObjectId`, the default aggregate key)
- `Microsoft.CodeAnalysis` 5.x (both source generators)
- AutoMapper, xUnit, Dapr / DotNetCore.CAP

## References

- `README.md` — extensive Chinese user documentation; keep it in sync when changing public behavior (it contains some stale spots, e.g. compiled models).
- Known review register: the old `MSFramework-code-review.md` has been deleted; known P0/P1 topics (domain-event clearing semantics, non-atomic multi-DbContext UoW, Dapr auth fail-open, MySQL migration FK removal) are now documented in the comments of the relevant sources (e.g. `EfUnitOfWork.SaveChangesAsync`, `DaprSecurityMiddleware`, `MigrationsSqlGenerator`) — check those comments before assuming a suspicious behavior is a bug.
- Nested guides: `src/MSFramework/AGENTS.md`, `src/MSFramework.AspNetCore/AGENTS.md`, `src/MSFramework.Ef/AGENTS.md`.
- `dapr/` — local Dapr config/components for the Ordering.API sample (the distributed-transaction sample has been removed).
