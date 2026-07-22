# MSFramework.Ef

**Parent**: [Root AGENTS.md](../../AGENTS.md)
**Generated:** 2026-07-07 | Commit: 8505696 | Branch: master

## OVERVIEW

EF Core integration layer providing `DbContextBase`, `EfRepository`, entity configuration extensions, soft-delete, auditing, JSON column mapping, and Unix timestamp storage. Also now hosts `DesignTimeDbContextFactoryBase` (moved from `MSFramework.Ef.Design`).

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Base DbContext | `DbContextBase.cs` | Wraps domain event dispatch, audit, UoW |
| DbContext factory | `DbContextFactory.cs` | For primary-constructor DI in repositories |
| Repository base | `Repositories/EfRepository.cs` | `EfRepository<TEntity, TKey>` |
| Entity configuration base | `EntityTypeConfigurationBase.cs` | Typed base class: `EntityTypeConfigurationBase<TEntity, TContext>` |
| Audit property config | `Extensions/EntityTypeBuilderExtensions.cs` | `ConfigureAuditProperties()`, `ConfigureCreation()`, etc. |
| JSON column mapping | `Extensions/JsonPropertyBuilderExtensions.cs` | `.UseJson()` for complex properties |
| Unix timestamp | `Extensions/UnixTimePropertyExtensions.cs` | `.UseUnixTime(milliseconds: false)` |
| Soft delete filter | `Extensions/SoftDeleteQueryExtensions.cs` | Auto-filter `IsDeleted` on `IDeletion` entities |
| Enumeration mapping | `Extensions/EnumerationPropertyExtensions.cs` | Maps `Enumeration` subtypes to varchar |
| Design-time factory | `DesignTimeDbContextFactoryBase.cs` | Moved from MSFramework.Ef.Design |
| DbContext settings | `DbContextSettings.cs` | `appsettings.json` → `DbContexts` section |
| Migration initializer | `Initializer/` | Auto-migration, seeding |

## PATTERNS

### DbContextBase
```csharp
public class OrderingContext(DbContextOptions<OrderingContext> options, IServiceProvider sp)
    : DbContextBase(options, sp) { }
```
- Auto-dispatches domain events on `SaveChangesAsync`
- Integrates audit logging (if `IAuditingStore` registered)
- Provides `IUnitOfWork` implementation

### EfRepository
```csharp
public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository { }
```
- `DbContextFactory` is a thin wrapper allowing scoped DbContext resolution
- `Store` property = `DbSet<TEntity>` for LINQ queries
- `GetDbContextAsync()` for when you need the full context

### EntityTypeConfigurationBase
```csharp
public class OrderConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ConfigureAuditProperties();
        builder.HasQueryFilterSoftDelete();
    }
}
```

### Key Extension Methods

| Method | What it does |
|--------|-------------|
| `.UseJson()` / `.UseJson(JsonDataType)` | Serialize property as JSON column (JSONB for PG, JSON for MySQL) |
| `.UseUnixTime(milliseconds)` | Store `DateTimeOffset` as bigint Unix timestamp |
| `.ConfigureAuditProperties()` | Add creation/modification/deletion tracking columns |
| `.ConfigureCreation()` | Add creation audit only |
| `.ConfigureModification()` | Add modification audit only |
| `.ConfigureDeletion()` | Add soft-delete support |
| `.HasQueryFilterSoftDelete()` | Auto-filter `IsDeleted == false` |
| `builder.UseEnumeration()` | Map Enumeration subclass — auto-configures varchar column |

### DbContextSettings (appsettings.json)
```json
{
  "DbContexts": {
    "MyApp.Infrastructure.MyContext": {
      "DatabaseType": "PostgreSql",
      "TablePrefix": "t_",
      "UseUnderScoreCase": true,
      "QuerySplittingBehavior": "SplitQuery"
    }
  }
}
```
Loaded via `options.Load(settings)` in `OnConfiguring`.

## NOTES
- `DesignTimeDbContextFactoryBase` lives here now — `MSFramework.Ef.Design` is nearly empty
- Soft-delete entities MUST implement `IDeletion` — the query filter applies automatically
- JSON column mapping: for interface/abstract types, pass the concrete runtime type to `UseJson(typeof(...))`
