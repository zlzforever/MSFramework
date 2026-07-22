# MSFramework Core

**Parent**: [Root AGENTS.md](../../AGENTS.md)
**Generated:** 2026-07-07 | Commit: 8505696 | Branch: master

## OVERVIEW

Core framework package providing DDD primitives, CQRS mediator, DI auto-registration, local event bus, and common utilities. Referenced by all other MSFramework packages.

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Aggregate roots / entities | `Domain/` | Creation-, Modification-, DeletionAggregateRoot |
| Value objects | `Domain/ValueObject.cs` | `record`-based, `Copy()` via `with` |
| Mediator / CQRS | `Mediator/` | `Request`, `Request<TResponse>`, `IMediator` |
| DI auto-registration | `Extensions/DependencyInjection/` | Marker interfaces + `ServiceCollectionExtensions` |
| Local event bus | `LocalEvent/` | In-process pub/sub |
| ApiResult types | `Common/ApiResult.cs` | Moved from MSFramework.AspNetCore.Mvc |
| Argument validation | `Utils/Check.cs` | `Check.NotNull`, `Check.NotNullOrEmpty` |
| Framework builder | `MicroserviceFrameworkBuilder.cs` | Extension point for `AddMicroserviceFramework()` |
| Framework exceptions | `MicroserviceFrameworkException.cs`, `MicroserviceFrameworkFriendlyException.cs` | Error code + message |

## MODULE MAP

```
src/MSFramework/
├── Domain/         # 25 files - DDD primitives
│   ├── EntityBase.cs, IEntity.cs, IKey.cs
│   ├── CreationEntity.cs, ModificationEntity.cs, DeletionEntity.cs
│   ├── CreationAggregateRoot.cs, ModificationAggregateRoot.cs, DeletionAggregateRoot.cs
│   ├── IAggregateRoot.cs, IOptimisticLock.cs
│   ├── DomainEvent.cs, IDomainEventHandler.cs
│   ├── ValueObject.cs  (abstract record)
│   ├── Enumeration.cs  (base class for enum-like types)
│   ├── IRepository.cs, IUnitOfWork.cs
│   ├── IObjectAssembler.cs  (DTO mapping)
│   └── IDomainService.cs, IExternalEntityRepository.cs
├── Mediator/        # 4 files - CQRS
│   ├── Request.cs, Request<TResponse> (command/event base types)
│   ├── IRequestHandler.cs, IRequestHandler<TRequest, TResponse>
│   └── Mediator.cs (Expression.Compile-based dispatch)
├── Extensions/
│   ├── DependencyInjection/  # ITransientDependency, IScopeDependency, ISingletonDependency
│   │   └── LifetimeUtilities.cs (auto-registration via assembly scanning)
│   └── Options/              # IOptions, IConfigureOptions
├── LocalEvent/       # In-process pub/sub (9 files)
├── Common/           # ApiResult, ApiResultWithErrors (6 files)
├── Application/      # IApplicationInfo
├── Auditing/         # AuditOperation, AuditEntity, AuditProperty models
├── Text/Json/        # System.Text.Json converters (5 converters)
├── Utils/            # Check.NotNull, cryptography, IP helpers
├── Collections/      # PagedResult, IPagedRequest
├── Linq/             # Expression helpers
├── Runtime/          # OS helpers
├── Serialization/    # IJsonSerializer abstraction
└── Security/Claims/  # ClaimsPrincipal extensions
```

## CONVENTIONS

### Aggregate Root Hierarchy
```
EntityBase<TKey>
├── CreationEntity<TKey>          → CreationAggregateRoot<TKey>
├── ModificationEntity<TKey>      → ModificationAggregateRoot<TKey>
└── DeletionEntity<TKey>          → DeletionAggregateRoot<TKey>
```
Choose based on auditing needs. `ModificationAggregateRoot` (no TKey param) defaults to `ObjectId`.

### Domain Events
- Raise via `AddDomainEvent()` on aggregate root BEFORE SaveChanges
- Handlers implement `IDomainEventHandler<T>` where T : DomainEvent
- Events are dispatched in same UoW transaction

### Mediator (CQRS)
- `Request` = fire-and-forget or publish (no response)
- `Request<TResponse>` = command with response
- `SendAsync()` = single handler; `PublishAsync()` = all registered handlers
- Handler dispatch uses pre-compiled `Expression.Compile` delegates, NOT `MethodInfo.Invoke`

### DI Auto-Registration
- Implement `ITransientDependency`, `IScopeDependency`, or `ISingletonDependency`
- Assembly scanning picks up all implementations automatically
- `AddMicroserviceFramework(x => x.UseDependencyInjectionLoader())`

## ANTI-PATTERNS

- Do NOT use public constructors for aggregate roots — use `static Create()/New()` factory methods
- Do NOT dispatch domain events outside the UoW boundary
- Do NOT reference infrastructure packages from Domain layer
- NEVER suppress type errors with `as any`, `@ts-ignore` in C# — use explicit casts or null checks

## UNIQUE STYLES

- `ApiResult<T>` uses implicit operator from T: `ApiResult<string> result = "hello";`
- `ObjectId` (MongoDB.Bson) is the default aggregate root key when TKey not specified
- `Mediator` is `internal sealed` — only accessible via `IMediator` interface
