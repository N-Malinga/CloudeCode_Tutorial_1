---
name: project-architecture
description: Layer roots, .csproj reference graph, allowed NuGet per layer, and known legitimate exceptions for the ProductManagement solution
metadata:
  type: project
---

## Layer roots (under src/)

- Domain:         `src/ProductManagement.Domain`
- Application:    `src/ProductManagement.Application`
- Infrastructure: `src/ProductManagement.Infrastructure`
- API:            `src/ProductManagement.API`

Test projects under `tests/`:
- `tests/ProductManagement.Domain.Tests`
- `tests/ProductManagement.Application.Tests`

## .csproj reference graph (verified)

- Domain          — no ProjectReferences
- Application     — ProjectReference → Domain only
- Infrastructure  — ProjectReference → Application, Domain
- API             — ProjectReference → Application, Infrastructure

## Allowed NuGet per layer (verified from .csproj files)

**Domain:** MediatR only (for INotification on domain events).

**Application:** MediatR, FluentValidation, FluentValidation.DependencyInjectionExtensions,
Microsoft.Extensions.DependencyInjection.Abstractions.
- `Microsoft.Extensions.DependencyInjection` usage in `DependencyInjection.cs` is legitimate —
  it is the `AddApplication()` extension method wiring.

**Infrastructure:** EF Core, Microsoft.EntityFrameworkCore.Design (tooling requirement),
plus Application and Domain project references.

**API:** Microsoft.EntityFrameworkCore.Design (EF tooling on startup project). All others via
Application and Infrastructure references.

## Legitimate exceptions

- `DependencyInjection.cs` in Application uses `Microsoft.Extensions.DependencyInjection` —
  this is permitted (the .csproj references the Abstractions package, and DI wiring must live
  somewhere in Application for `AddApplication()`).
- `**/Migrations/*.cs` files are EF-generated and exempt from file-scoped namespace and other
  style rules.
- `Program.cs` in API may call `AddInfrastructure(builder.Configuration)` — legitimate
  composition-root wiring.
- `Microsoft.EntityFrameworkCore.Design` appears on both Infrastructure and API — required by
  EF tooling on the startup project.

## Established four-file CQRS pattern

CLAUDE.md says "three files" per use-case folder (Query/Command, Validator, Handler).
In practice, both `BulkAdjustPrice` (command) and `GetProductCount` (query) use a fourth
`*Result.cs` file for a dedicated result DTO. This is an existing codebase convention —
not a violation — when the response type is use-case-specific and does not fit the generic
`ProductDto` / `PagedResult<ProductDto>` types.

## IProductRepository.CountAsync

`CountAsync(ProductCategory? category, bool? isActive, CancellationToken)` was added to
`IProductRepository` to support the GetProductCount query. This follows the correct pattern:
abstraction defined in Application, implementation lives in Infrastructure.
