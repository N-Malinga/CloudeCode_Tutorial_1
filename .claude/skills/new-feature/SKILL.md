---
name: new-feature
description: Scaffold a new CQRS feature (command or query) for the Product Management API. Use whenever the user asks to add a new use case like "add ArchiveProduct command", "create a GetProductsByCategory query", or "scaffold a new feature". Handles the full set of files (request, handler, validator, controller endpoint, handler tests, validator tests) following Clean Architecture and the conventions in CLAUDE.md.
---

# new-feature — CQRS feature scaffolder

## When to use
The user wants to add a new command or query to the Application layer.
Typical triggers: "add X command", "scaffold Y query", "new feature: Z".

## What you need before scaffolding
Confirm these with the user if not already stated. Do not guess silently — ask
once, then proceed:

1. **Feature name** in PascalCase (e.g. `ArchiveProduct`, `GetProductsByCategory`).
2. **Type**: `Command` or `Query`.
3. **HTTP route and verb** for the endpoint.
4. **Request body / route / query parameters** — exact fields and types.
5. **Validation rules** — which fields, which constraints.
6. **Domain behaviour** — which aggregate methods are called; any new domain
   events; any invariants to enforce on the aggregate side.
7. **Return shape** — what the API should respond with on success and on
   each failure mode.

## Procedure

1. Read `CLAUDE.md` and confirm the architectural rules still hold.
2. Read one existing feature in the same folder family as a style reference
   (e.g. `src/ProductManagement.Application/Features/Products/CreateProduct/`).
3. Generate each file from the matching file in `templates/`, substituting
   `{{FeatureName}}`, `{{Type}}` (Command|Query), and the per-feature
   specifics gathered above.
4. Files to create (path uses `{{FeatureName}}` and `{{Type}}`):
   - `src/ProductManagement.Application/Features/Products/{{FeatureName}}/{{FeatureName}}{{Type}}.cs`
   - `src/ProductManagement.Application/Features/Products/{{FeatureName}}/{{FeatureName}}{{Type}}Handler.cs`
   - `src/ProductManagement.Application/Features/Products/{{FeatureName}}/{{FeatureName}}{{Type}}Validator.cs`
   - `tests/ProductManagement.Application.Tests/Features/Products/{{FeatureName}}/{{FeatureName}}{{Type}}HandlerTests.cs`
   - `tests/ProductManagement.Application.Tests/Features/Products/{{FeatureName}}/{{FeatureName}}{{Type}}ValidatorTests.cs`
5. Update `src/ProductManagement.API/Controllers/ProductsController.cs` —
   add the endpoint using the verb and route the user specified. Wire it
   through MediatR. Map domain exceptions to ProblemDetails via the global
   handler; do not catch them locally.
6. If the user described new domain behaviour (a new aggregate method, a
   new domain event, a new invariant), apply those changes to the Domain
   project as well — and add Domain-level unit tests for the invariants.
   See `templates/DomainTest.cs.template`.
7. Verify: run `dotnet build -warnaserror` and `dotnet test`. Report results.
8. Show the diff before saving. If anything is ambiguous, stop and ask.

## Constraints (non-negotiable)
- Inject `IProductRepository` and `IUnitOfWork`. Never `DbContext`.
- No `using Microsoft.EntityFrameworkCore` in the Application project.
- Tests use FluentAssertions (`.Should()...`), not raw `Assert.*`.
- Each handler test file covers at minimum: happy-path, validation-failure,
  not-found. Add more cases if the feature has additional failure modes
  (e.g. AdjustStock needs a "would go below zero" case).
- Validators cover every rule the user specified, with one test method per
  rule (positive and negative case each where it makes sense).

## Output checklist before reporting "done"
- [ ] All files created at the paths above
- [ ] Endpoint registered in ProductsController
- [ ] `dotnet build -warnaserror` passes
- [ ] `dotnet test` passes
- [ ] Diff shown to user before final save