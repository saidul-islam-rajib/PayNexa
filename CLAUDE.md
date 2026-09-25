# PayNexa — Project Instructions

## Code rules
- Never write comments of any kind: no `//`, `/* */`, XML doc comments (`///`), `<!-- -->` in `.csproj`/`.props`, or `#` comments in YAML, Dockerfiles and `.env` files. Express intent through naming and small, focused types and methods.
- Only free, open-source dependencies (MIT, Apache-2.0, BSD). Never add a package that needs a commercial license. Pin every version in `Directory.Packages.props`.
  - Mediator (martinothamar) instead of MediatR, manual mapping or Mapster instead of AutoMapper, Shouldly instead of FluentAssertions, NSubstitute instead of Moq.
- Write maintainable, reusable, industry-standard code that follows `doc/fintech_microservices_industry_requirements.md`.

- No hard-coded user-facing text: error codes, error messages and validation messages live in constants classes (`*ErrorCodes`, `*ErrorMessages`, `ValidationMessages`) and are referenced from there.
- Dependency injection is composed per layer, one `DependencyInjection.cs` per project: `builder.Services.AddPresentation(...).AddApplication().AddInfrastructure(builder.Configuration)`.
- Database-agnostic architecture: Domain and Application depend only on abstractions (repositories, read stores, `IUnitOfWork`, `IOutbox`, `ICacheService`, `IEventBus`, `IDatabaseInitializer`, `IDataSeeder`); concrete stores live in Infrastructure and building blocks (Dependency Inversion).
- Common things live in one place: cross-service patterns go to `src/BuildingBlocks`, service-wide ones to the service's `Common` folder. Audit fields (`CreatedAtUtc/By`, `UpdatedAtUtc/By`) and `Version` come from `AggregateRoot<TId>` and are stamped automatically — never set them by hand. Single-value value objects and ids derive from `SingleValueObject<T>` / `StronglyTypedId` and are mapped to columns by convention.
- Mapping uses Riok.Mapperly (compile-time, `RequiredMappingStrategy.Target`), in a `Mappings` folder per layer: API request → command/query, aggregate → response/snapshot, snapshot → read model.
- Controllers derive from `ApiControllerBase`: no route strings for the resource (`api/v{version}/[controller]`, version from the `Controllers.V<n>` namespace, kebab-case URLs) and no `[ProducesResponseType]` (added by convention from the HTTP method). Actions return `ActionResult<T>` via `Respond` / `RespondCreated`. Query-string models derive from `PagedRequest` and live in the API layer.
- Settings: every service has its own `appsettings.json` / `appsettings.Development.json`. Connection strings contain no password; the SQL password comes from `SqlServer:Password` (user-secrets locally, `.env` in Docker, Vault later).
- Events: aggregates raise domain events; application domain-event handlers turn them into integration events through the outbox; the outbox processor projects read models and publishes to Kafka. Add events whenever a state change matters to another component.

## Ports and developer experience
| Service | HTTPS | HTTP |
|---|---|---|
| API Gateway | 6000 | 5000 |
| Customer | 6001 | 5001 |
| Authentication | 6002 | 5002 |
| Payment | 6003 | 5003 |
| Transaction | 6004 | 5004 |
| Notification | 6005 | 5005 |

- Same host ports locally (`launchSettings.json`) and in Docker (containers listen on 8081/8080 and are mapped to these ports).
- Swagger UI opens automatically at `/swagger` in Development.
- In Development, startup creates/migrates databases, collections, indexes and Kafka topics, and seeds data when the store is empty.

## Docker
- The Compose project is always named `paynexa`; containers use fixed `container_name`s.
- Every change to a microservice includes its Docker updates (Dockerfile, `docker-compose.yml`, `docker-compose.override.yml`, `.env.example`), and the containers are rebuilt and running on the latest code.

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
