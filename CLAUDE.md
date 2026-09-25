# PayNexa — Project Instructions

## Code rules
- Never write comments of any kind: no `//`, `/* */`, XML doc comments (`///`), `<!-- -->` in `.csproj`/`.props`, or `#` comments in YAML, Dockerfiles and `.env` files. Express intent through naming and small, focused types and methods.
- Only free, open-source dependencies (MIT, Apache-2.0, BSD). Never add a package that needs a commercial license. Pin every version in `Directory.Packages.props`.
  - Mediator (martinothamar) instead of MediatR, manual mapping or Mapster instead of AutoMapper, Shouldly instead of FluentAssertions, NSubstitute instead of Moq.
- Write maintainable, reusable, industry-standard code that follows `doc/fintech_microservices_industry_requirements.md`.

## Documentation
- Every microservice has its own document in `doc/` (e.g. `doc/Customer.md`). Update it in the same change as the code.
- Shared building blocks are documented in `doc/BuildingBlocks.md`.
- Explanations belong in `doc/`, never in code comments.

## Solution layout
- Solution: `src/PayNexa.slnx`. Services live in `src/Services/<Service>/<Service>.{API,Application,Domain,Infrastructure,Contracts}`.
- Shared technical code lives in `src/BuildingBlocks/`. Unit tests live in `tests/Unit/`.
- Service projects set `<RootNamespace>PayNexa.<PluralContext>.<Layer></RootNamespace>` (e.g. `PayNexa.Customers.Domain`) so aggregates like `Customer` never clash with a namespace.
- Every API uses `AddPayNexaServiceDefaults()` / `UsePayNexaServiceDefaults()`; see `doc/BuildingBlocks.md` for logging, errors, outbox and service-client conventions.
- EF Core is code first; `dotnet-ef` is pinned in `dotnet-tools.json`. Strip the generated comment lines from new migrations.

## Data rules (requirements §5.1)
- Writes (POST/PUT/PATCH/DELETE) go to **SQL Server** only. SQL Server is the source of truth for every service.
- Reads (GET) are served from **MongoDB** read models, projected from SQL Server through the transactional outbox by a background processor. Commands never write to MongoDB directly.
- **Money is always read from the source of truth.** Payments, amounts, balances and any decision based on a monetary value are read from SQL Server, never from MongoDB or Redis.
- Redis caches sit in front of MongoDB read models only and never hold monetary values.
- Projections are idempotent and version-checked; command responses are built from the write model.
