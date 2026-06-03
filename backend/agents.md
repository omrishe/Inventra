# What AI agents should know about me

C# / ASP.NET Core developer building **Inventra** — a multi-tenant SaaS inventory system on .NET 10.

## Code Style

- **Explicit types**: No `var` unless the type is truly obvious.
- **Soft deletes only**: Entities must never be hard-deleted. Use `IsDeleted bool` and filter in queries.
- **Modern C#**: Use primary constructors, collection expressions (`[]`, `[..]`), pattern matching, `new()`.
- **No `field` keyword**.
- **UTC only**: Prefer `DateTimeOffset.UtcNow` or `DateTime.UtcNow` with `Kind.Utc`.
- **Prefer `IReadOnly*`** interfaces for collections (e.g., `IReadOnlyList<T>`, `IReadOnlyDictionary<K, V>`).
- **`static` when possible**: Mark methods/properties/local functions as `static` when they don't access instance members.
- **PascalCase every identifier** (types, members, variables, parameters). Exception: simple loop counters.
- Tabs for indentation; opening brace on its own line.
- Nullability **enabled**; treat warnings as errors.
- Use **file-scoped namespaces**.

## Architecture

- **Clean Architecture**: API → App → Domain.
  - **Domain** = pure (entities, enums, exceptions, interfaces).
  - **App** = business logic + interfaces, DTOs.
  - **Infra** = EF Core + external systems (migrations, external APIs).
  - **API** = thin layer (Controllers, Middleware).
- **Architecture Rule**: Dependencies must always flow inward.
- **EF Core** is allowed only in the Infrastructure layer.
- The system must **scale horizontally**.

## Multi-Tenancy

- **Shared DB, ChainId isolation**.
- Enforced automatically via **EF Core Global Query Filters**.
- **ChainId** must be populated **only from JWT**.

## EF Core

- Use `.AsNoTracking()` for all read-only queries.
- **Projection (`Select`) preferred** over returning entities.
- **xmin** for concurrency (PostgreSQL system column) — wired on `InventoryItem` via `UseXminAsConcurrencyToken()`.
- `AppDbContext` accessed via `IAppDbContext` interface in App layer.

## Multi-Tenant Isolation — Inventory Entities Exception

- `InventoryItem`, `StockMovement`, and `Reservation` do **not** implement `ITenantEntity` and carry **no `ChainId` column**.
- Chain boundary is enforced via the `Store` navigation: every query on these entities **must** include `.Where(x => x.Store.ChainId == tenantContext.ChainId)`.
- Store-level scope is applied on top: if `ITenantContext.StoreId != null`, add `.Where(x => x.StoreId == tenantContext.StoreId.Value)`.
- Missing this predicate is a **security regression** — treat it the same as a missing global query filter.

## API

- REST-based, prefixed with `/api/v1/...`.
- **DTOs only**: Never return or accept Domain entities in requests/responses.
- **Pagination required** for collection endpoints.
- **No tenant in routes**: The tenant is implicitly known via JWT.

## Mapping

- **Default**: Projection via LINQ `.Select()`.
- **Manual mapping** only when absolutely needed.
- **No AutoMapper** on hot-paths.

## Background Jobs

- Must be **stateless and idempotent**.
- **Tenant-safe loops**: Carefully handle loops over tenants to avoid data leaks.
- **Failure isolation per tenant**: A failure in one tenant should not cascade.

## Caching

- **Key**: Must always include `ChainId`.
- Use the **cache-aside pattern**.
- **Never cache entities**: Cache DTOs or primitive shapes only.

## Auth

- **JWT lifespan**: 15 minutes.
- **Refresh tokens**: 7 days lifespan, rotated, and hashed before storage.
- **JWT payload**: Must include `UserId`, `ChainId`, and `Role`.
- **No tokens in logs**: Ensure credentials/tokens are never written to logs.

## Observability

- Comprehensive **logs, metrics, and traces**.
- **ChainId + CorrelationId required** in log contexts.
- Alerting should be **metrics-based only**.

## Reliability

- **Stateless API**.
- Use **retries only for transient errors**.
- **Circuit breakers required** for external dependencies.
- Ensure **graceful degradation** of non-critical features.

## Consistency

- **Strong writes** for critical paths (e.g., inventory, reservations).
- **Eventual consistency** is acceptable for analytics.
- **Optimistic concurrency** using PostgreSQL `xmin`.

## Deployment

- **CI/CD required**.
- **3 environments** (e.g., Dev, Staging, Prod).
- Migrations must be **backward compatible**.
- **Rollback required** (system must support safe rollbacks).

## Frontend (React + Vite + TypeScript)

- **Stack**: Vite + React + TypeScript (strict), React Router v6, TanStack Query v5, Zustand.
- **Styling**: Vanilla CSS only (CSS Variables + CSS Modules).
- **Zero `any` policy**: Explicit typing for everything.
- **Server state**: Use `useQuery` and `useMutation`.
- **Client state**: Zustand strictly for client-only state.
- **Axios**: Single `apiClient` instance with token refresh interceptor.

---

# How I want AI agents to respond

- When unsure, ask clarifying questions first before proceeding.
