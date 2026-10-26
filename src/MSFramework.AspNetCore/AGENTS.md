# MSFramework.AspNetCore

**Parent**: [Root AGENTS.md](../../AGENTS.md)
**Generated:** 2026-07-07 | Commit: 8505696 | Branch: master

## OVERVIEW

ASP.NET Core integration providing unified API response wrapping, global exception handling, audit/UoW action filters, Dapr security middleware, and controller base class.

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Controller base | `ApiControllerBase.cs` | Inherit all API controllers from this |
| Response wrapping | `Filters/ResponseWrapperFilter.cs` | Wraps return values in `ApiResult<T>` |
| Exception handling | `Filters/GlobalExceptionFilter.cs` | Catches unhandled exceptions, returns ApiResult |
| Unit of work filter | `Filters/UnitOfWork.cs` | Auto-commits UoW after action |
| No UoW filter | `Filters/NoUnitOfWork.cs` | Opt-out for read-only actions |
| Audit attribute | `Filters/AuditAttribute.cs` | Attribute-based audit trigger |
| Filter registration | `Filters/ServiceCollectionExtensions.cs` | `.AddFilters()` extension |
| Dapr security | `DaprSecurityMiddleware.cs` | **DEPRECATED** — use official App API Token: daprd+app set `APP_API_TOKEN`; `AddAuthentication().AddDapr()` + `AddAuthorization(o => o.AddDapr())`; `[Authorize(Policy = "Dapr")]` on topic endpoints. Block `/dapr/subscribe` at the gateway |
| Service locator | `HttpContextScopeServiceProvider.cs` | Scoped IServiceProvider from HttpContext |
| Session | `HttpSession.cs` | Per-request session/correlation data |
| Identity | Core package: `MSFramework/IdentityModel/`, `MSFramework/Security/Claims/` | NOT here (moved) |
| Model binding | `Mvc/ModelBinding/` | Custom model binders |
| ApiResult types | Now in `MSFramework.Common` — NOT here | Moved to core package |

## PATTERNS

### Response Wrapping
All controller actions are auto-wrapped unless decorated with `[NoUnitOfWork]` or return `IActionResult` directly:
```json
// Action returns `string` → wrapped
{ "success": true, "code": 0, "msg": "", "data": "hello" }

// Action returns `ApiResult<T>` directly → NOT double-wrapped
```

### Global Exception Filter
- `MicroserviceFrameworkFriendlyException` → user-friendly message + 400
- Other exceptions → generic error + 500
- Response always uses `ApiResult` format

### Filter Order
1. `GlobalExceptionFilter` (first)
2. `UnitOfWork` / `NoUnitOfWork`
3. `ResponseWrapperFilter` (last)

### UoW Filter
- `UnitOfWork` is a **global implicit-commit filter**: it commits `IUnitOfWork` after a successful
  action for write HTTP methods (POST/PUT/PATCH/DELETE) only — read methods are not committed.
- `[NoUnitOfWork]` (class or action) opts out of the auto-commit.
- If an action throws, UoW is NOT committed. There is no `[UnitOfWork]` attribute — commit is the default.
- **Important**: multi-DbContext writes are NOT atomic (only the primary context is committed; see
  `EfUnitOfWork.SaveChangesAsync` comments); read endpoints need no opt-out (not committed anyway).

### Extensions
```csharp
services.AddMicroserviceFramework(x => x.UseAspNetCoreExtension());
// Registers: IHttpContextAccessor, HttpSession, filters, Dapr security
```

## NOTES
- `ApiResult` and `ApiResultWithErrors` are now in `MicroserviceFramework.Common` (core package), not here
- The `ResponseWrapperFilter` still references them from the new namespace
- `DaprSecurityMiddleware` is deprecated: use the official App API Token scheme (`APP_API_TOKEN` env on daprd+app, `AddAuthentication().AddDapr()`); it now only enforces the loopback network-layer check and its comment documents the full security model
