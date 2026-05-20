# Product Management Web API

Clean Architecture sample on .NET 10 with CQRS (MediatR), FluentValidation, EF Core 10 (SQL Server LocalDB), and OpenAPI via Microsoft.AspNetCore.OpenApi + NSwag UI.

## Prerequisites
- .NET 10 SDK (10.0.300+)
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef --version 10.0.0`

## Quick start

```pwsh
dotnet restore
dotnet build
dotnet ef database update --project src/ProductManagement.Infrastructure --startup-project src/ProductManagement.API
dotnet run --project src/ProductManagement.API
```

Then browse `https://localhost:<port>/swagger` for the API explorer.

## Run tests

```pwsh
dotnet test
```

## Project layout

- `src/ProductManagement.Domain` — entities, value objects, domain events
- `src/ProductManagement.Application` — CQRS handlers, DTOs, validators, abstractions
- `src/ProductManagement.Infrastructure` — EF Core, repositories, migrations
- `src/ProductManagement.API` — controllers, DI composition, OpenAPI/Swagger UI
- `tests/ProductManagement.Domain.Tests`
- `tests/ProductManagement.Application.Tests`
