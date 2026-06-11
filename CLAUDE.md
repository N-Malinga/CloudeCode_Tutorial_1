# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project

Product Management Web API on **.NET 10** using Clean Architecture, CQRS (MediatR 12), FluentValidation, EF Core 10 (SQL Server LocalDB), and OpenAPI via `Microsoft.AspNetCore.OpenApi` + NSwag UI.

Solution: `ProductManagement.sln` — 4 source projects + 2 test projects, with Central Package Management (`Directory.Packages.props`) and `TreatWarningsAsErrors` in Release.

---

## Architectural boundaries

```
Domain          ← no dependencies on other projects
Application     ← Domain
Infrastructure  ← Application, Domain
API             ← Application, Infrastructure
```

**Rules — do not break these:**

- **Domain** is pure C#. It must not reference EF Core, ASP.NET, FluentValidation, or `Microsoft.Extensions.*`. The only NuGet allowed is `MediatR` (for the `INotification` marker on domain events).
- **Application** depends only on Domain. It defines `IProductRepository` and `IUnitOfWork`; it never references `DbContext` or EF Core types.
- **Infrastructure** implements Application interfaces and owns EF Core (`ProductManagementDbContext`, migrations, repositories, `UnitOfWork`). It does not reference the API.
- **API** is composition-only. Controllers translate HTTP → MediatR; no business logic lives here. `Program.cs` calls `AddApplication()` and `AddInfrastructure(builder.Configuration)`.
- Add a new dependency only if it respects the arrows above. If you find yourself wanting Application → Infrastructure, the abstraction belongs in Application instead.

**Cross-cutting:**

- Domain events extend `IDomainEvent` (which is `MediatR.INotification`). They are raised on entities and dispatched by `UnitOfWork.SaveChangesAsync` *after* the DB write succeeds.
- Exceptions: application failures derive from `AppException` (in `Application/Common/Exceptions`), a "smart" base exposing `StatusCode` + `Title` — `NotFoundException` → 404, `BadRequestException` → 400, `ValidationException` → 400 (with per-field errors). `DomainException` lives in the Domain layer (so it can't derive from `AppException`) and is mapped to 422 explicitly. All are turned into RFC 7807 `ProblemDetails` by `GlobalExceptionHandler` (an `IExceptionHandler` registered via `AddExceptionHandler` + `UseExceptionHandler`). Each response carries a `traceId` (added centrally via `AddProblemDetails(CustomizeProblemDetails)`) that matches the log entry; in .NET 10 the middleware suppresses its own diagnostics once the handler returns `true`, so there are no duplicate logs. Unexpected (500) details are hidden outside Development.

---

## CQRS conventions

Every use case is a separate folder under `Application/Products/Commands/<Name>/` or `Application/Products/Queries/<Name>/` with **three files**:

| File | Type | Purpose |
|---|---|---|
| `<Name>Command.cs` / `<Name>Query.cs` | `record` implementing `IRequest` or `IRequest<TResponse>` | Input contract |
| `<Name>Validator.cs` | `AbstractValidator<TCommand>` | FluentValidation rules |
| `<Name>Handler.cs` | `IRequestHandler<TCommand[, TResponse]>` | Business logic |

**Rules:**

- One command/query per file. Never reuse a `*Command` across handlers.
- Validators run *before* handlers via `ValidationBehavior<TRequest, TResponse>` (registered in `Application.DependencyInjection`). Throw nothing from validators; just declare rules.
- Handlers may assume the request already passed validation. They are responsible for **lookups** (throwing `NotFoundException` when missing) and for **delegating mutations to the aggregate** (`Product.Update`, `Product.ChangePrice`, etc.) — never mutate domain state from a handler directly.
- Handlers depend on `IProductRepository` + `IUnitOfWork`. They must call `unitOfWork.SaveChangesAsync(cancellationToken)` exactly once on success.
- Queries return DTOs (`ProductDto`, `PagedResult<ProductDto>`) — never domain entities. Use `ProductDto.FromProduct(...)` for projection.
- Controllers send via `ISender`. They do not call repositories.

---

## Naming rules

- **Namespaces** mirror folders (`ProductManagement.Application.Products.Commands.CreateProduct`).
- **Namespace declarations** are file-scoped (`namespace X;`). Block-scoped is allowed *only* in `**/Migrations/*.cs` (EF-generated).
- **Files**: one public type per file, file name matches the type.
- **CQRS suffixes**: `*Command`, `*Query`, `*Handler`, `*Validator`. Domain events end in `*Event` (e.g. `ProductCreatedEvent`).
- **Records** for immutable data: commands, queries, DTOs, domain events. **Classes** for entities, value objects, validators, handlers, exceptions.
- **Value objects** are sealed and inherit `ValueObject`. They expose a static `Create(...)` factory that throws `DomainException` on invariant violation; constructors are private.
- **Aggregates** (e.g. `Product`) are sealed and inherit `Entity`. They expose a static `Create(...)` factory and intent-revealing methods (`ChangePrice`, `DepleteStock`, `Update`, `Deactivate`) — no public setters.
- **DI extension methods** live in a `DependencyInjection.cs` at the project root and follow `AddApplication()` / `AddInfrastructure(IConfiguration)`.

---

## Testing requirements

- **xUnit + FluentAssertions 7.2.0 + Moq.** FluentAssertions stays on 7.x (Apache-2.0); do not bump to 8+ without flagging the licensing change.
- **Two projects, mirror the source layout:**
  - `tests/ProductManagement.Domain.Tests` — covers aggregate invariants and value-object equality only. No mocks.
  - `tests/ProductManagement.Application.Tests` — covers handlers and validators. Uses `Moq` doubles via `HandlerTestBase`. No EF Core, no HTTP.
- **Coverage rules — these are the bar to merge:**
  - Every domain invariant has a test (name length, price > 0, stock ≥ 0, currency immutability, depletion event firing).
  - Every handler has **at least one happy-path test and one failure test** (e.g. `Handle_MissingProduct_ThrowsNotFound`).
  - Every command/query has a validator test exercising each `RuleFor` (use `FluentValidation.TestHelper`).
- **Discipline:**
  - Don't test the framework. Don't assert against `Moq` call ordering unless the order is part of the contract.
  - Use `MockBehavior.Strict` (the default in `HandlerTestBase`) so unexpected calls fail loudly.
  - New use case = new handler tests + new validator tests, same PR.

---

## Commands

### Build, test, run

```pwsh
dotnet restore
dotnet build                        # Debug
dotnet build -c Release             # must finish with 0 warnings, 0 errors
dotnet test                         # both test projects
dotnet test -c Release --no-build

dotnet run --project src/ProductManagement.API
# Swagger UI:  http://localhost:5104/swagger
# OpenAPI doc: http://localhost:5104/openapi/v1.json
```

### EF Core migrations (LocalDB)

`dotnet-ef` 10.0.8 global tool is required (`dotnet tool install --global dotnet-ef --version 10.0.8`).

```pwsh
# Create a new migration after changing the model
dotnet ef migrations add <Name> `
  --project src/ProductManagement.Infrastructure `
  --startup-project src/ProductManagement.API `
  --output-dir Persistence/Migrations

# Apply pending migrations
dotnet ef database update `
  --project src/ProductManagement.Infrastructure `
  --startup-project src/ProductManagement.API

# Roll back the last migration
dotnet ef migrations remove `
  --project src/ProductManagement.Infrastructure `
  --startup-project src/ProductManagement.API
```

Notes:
- `Microsoft.EntityFrameworkCore.Design` is referenced from **both** `Infrastructure` and `API` — the tooling requires it on the startup project.
- Connection string is configured in `src/ProductManagement.API/appsettings.json` under `ConnectionStrings:DefaultConnection`. Default points to `(localdb)\MSSQLLocalDB`.
- Migration files are EF-generated and are exempted from the `IDE0161` file-scoped-namespace rule via `.editorconfig`. Do not hand-edit them to change the namespace style — re-run `dotnet ef` to regenerate instead.

### Package versions

Versions are managed centrally in `Directory.Packages.props`. Add a `<PackageVersion>` there and reference it with `<PackageReference Include="X" />` (no Version attribute) in the consuming `.csproj`. Do not pin versions on `PackageReference` items — Central Package Management will reject the build.
