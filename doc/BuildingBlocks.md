# Building Blocks

Shared technical infrastructure in `src/BuildingBlocks/`. Building blocks contain **no business or domain code** (requirements §51). Every service composes them the same way, so logging, errors, persistence, messaging and resilience behave identically across the platform.

| Project | Responsibility | Referenced by |
|---|---|---|
| `PayNexa.SharedKernel` | DDD base types (`Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `StronglyTypedId`, `DomainEvent`, `DomainException`) and `Result`/`Error` | Every `*.Domain` |
| `PayNexa.Common` | Mediator pipeline behaviors, `OperationStep` logging, correlation context, pagination/sorting, domain-event dispatch, persistence/cache/initialization abstractions, `ValidationMessages` | Every `*.Application` |
| `PayNexa.Messaging.Abstractions` | `IIntegrationEvent`, `[IntegrationEvent]`, `IEventBus`, message headers, serializer — no dependencies | Every `*.Contracts` |
| `PayNexa.Messaging` | Kafka event bus, outbox → Kafka publishing, topic creation, health check | Every `*.Infrastructure` |
| `PayNexa.Logging` | Serilog (Console + Seq + rolling file), correlation middleware, request logging, masking | Every host |
| `PayNexa.Observability` | OpenTelemetry, health endpoints, service-to-service HTTP pipeline | Every host |
| `PayNexa.AspNetCore` | `AddPayNexaServiceDefaults()`, Problem Details, exception handler, versioning, OpenAPI + Swagger UI, `InitializeInfrastructureAsync()`, paging query parameters | Every `*.API` |
| `PayNexa.SqlServer` | Write store: EF Core, command logging, unit of work (+ domain events), transactional outbox, migrations initializer, EF paging | Every `*.Infrastructure` |
| `PayNexa.MongoDb` | Read store: client, command logging, collection/index initializer, version-checked projections, Mongo paging | Every `*.Infrastructure` |
| `PayNexa.Caching` | Redis cache-aside (`ICacheService`); an outage degrades to a cache miss | Every `*.Infrastructure` |
| `PayNexa.Vault` | HashiCorp Vault secrets (planned) | — |

Dependency direction: `Domain → SharedKernel` · `Contracts → Messaging.Abstractions` · `Application → Common, Domain, Contracts` · `Infrastructure → Application + SqlServer/MongoDb/Caching/Messaging` · `API → AspNetCore + Application + Infrastructure`. Domain and Application never see a database, cache or broker type — only abstractions (Dependency Inversion).

---

## 1. Service composition (dependency injection per layer)

Each layer owns a `DependencyInjection.cs`, and `Program.cs` composes them (the Dinner_Host pattern):

```csharp
Log.Logger = PayNexaLogging.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.Services
            .AddPresentation(builder.Configuration, builder.Environment)
            .AddApplication()
            .AddInfrastructure(builder.Configuration);
    }

    var app = builder.Build();
    {
        app.UsePresentation();
        await app.InitializeInfrastructureAsync();
        await app.RunAsync();
    }

    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Service terminated unexpectedly during startup");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

| Layer | Method | Registers |
|---|---|---|
| API | `AddPresentation` / `UsePresentation` | Service defaults (logging, observability, Problem Details, controllers, versioning, OpenAPI, Swagger) and Mediator with the pipeline `LoggingBehavior → ValidationBehavior → DomainRuleBehavior` |
| Application | `AddApplication` | FluentValidation validators and domain-event handlers found in the assembly |
| Infrastructure | `AddInfrastructure` | Composed from `AddWriteStore`, `AddReadStore`, `AddCaching`, `AddMessaging` |

---

## 2. Domain model (DDD)

```text
Customer.Domain/
├── CustomerAggregate/
│   ├── Customer.cs                          AggregateRoot<CustomerId>
│   ├── ValueObjects/  CustomerId, PersonName, Email, PhoneNumber
│   ├── Enums/         CustomerStatus
│   └── Events/        CustomerRegisteredDomainEvent, CustomerProfileUpdatedDomainEvent
└── Common/Errors/
    ├── Errors.Customer.cs                   static partial class Errors { static class Customer { ... } }
    ├── CustomerErrorCodes.cs                constants
    └── CustomerErrorMessages.cs             constants
```

- **Aggregates** derive from `AggregateRoot<TId>`, expose behavior (`Register`, `UpdateProfile`) instead of setters, keep a `Version` for optimistic concurrency, and raise domain events with `Raise(...)`.
- **Strongly typed IDs** derive from `StronglyTypedId` (`CustomerId.CreateUnique()`, `CustomerId.Create(guid)`).
- **Value objects** derive from `ValueObject` (equality by components) and validate in their `Create` factory.
- **Invariant violations** throw `DomainException(Errors.Customer.X)`. `DomainRuleBehavior` converts it into a failed `Result`, so handlers contain no try/catch for domain rules.
- **Errors** are defined once per aggregate in `Errors.<Aggregate>` and built from `*ErrorCodes` / `*ErrorMessages` constants — no user-facing text is hard-coded anywhere else.

---

## 3. Events

```mermaid
flowchart LR
    Aggregate["Aggregate<br/>Raise(domain event)"] --> UoW["IUnitOfWork.SaveChangesAsync"]
    UoW -->|"1. dispatch before save"| Handler["Domain event handler<br/>(Application)"]
    Handler -->|"IOutbox.Enqueue(integration event)"| UoW
    UoW -->|"2. one SQL transaction"| SQL[("business rows + OutboxMessages")]
    SQL --> Processor["OutboxProcessor"]
    Processor --> Projection["Read-model projection (MongoDB)"]
    Processor --> Kafka["KafkaEventBus → topic paynexa.&lt;context&gt;"]
```

| Kind | Where | Purpose |
|---|---|---|
| Domain event | `*.Domain/.../Events`, derives `DomainEvent` | Something happened inside the aggregate; handled in-process before the transaction commits |
| Integration event | `*.Contracts/Events`, implements `IIntegrationEvent`, `[IntegrationEvent("customer.created", version: 1)]` | Published to other services through the outbox and Kafka |

Why this is safer than publishing directly from a handler:

- The integration event is committed **in the same SQL transaction** as the state change — no lost or phantom events when the broker or the process fails.
- `EventId` and `OccurredAtUtc` are **stable values** captured once (the domain event's identity becomes the integration event's identity), so consumers can deduplicate.
- Messages are **keyed by aggregate id**, so all events of one customer stay ordered on one partition.
- Payloads never carry secrets or card data; the partition key is transport metadata and is stripped from the JSON automatically.

### Kafka conventions

| Item | Convention |
|---|---|
| Topic | `paynexa.<bounded context>` derived from the event type (`customer.created` → `paynexa.customer`); override with `[IntegrationEvent(..., Topic = "...")]` |
| Key | `IIntegrationEvent.PartitionKey` (aggregate id) |
| Value | JSON (camelCase, enums as strings) |
| Headers | `event-id`, `event-type`, `event-version`, `occurred-at-utc`, `content-type`, `source-service`, `correlation-id`, `traceparent` |
| Producer | Idempotent, `acks=all`, LZ4 compression, 10 s delivery timeout (the outbox retries afterwards) |
| Topics | Created at startup in Development (`Kafka:AutoCreateTopics`), 3 partitions; broker auto-creation is disabled |

Registering in Infrastructure:

```csharp
services
    .AddPayNexaKafka(configuration)
    .AddIntegrationEventPublishing<CustomerCreatedIntegrationEvent>()
    .AddIntegrationEventPublishing<CustomerUpdatedIntegrationEvent>();
```

Consumers (inbox idempotency, retry and dead-letter topics, requirements §46) are added with the first consuming service.

---

## 4. Pagination and sorting

| Type | Layer | Role |
|---|---|---|
| `PageRequest(Page, PageSize)` | Common | Defaults (1, 20), max page size 100, `Skip` |
| `PagedResult<T>` | Common | `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPreviousPage`, `hasNextPage`, `Map()` |
| `SortRequest<TField>` / `SortDirection` | Common | Resolved sort instruction |
| `SortFieldMap<TField>` | Common | Maps API names (`created-at`) to a typed field, with a default sort and case-insensitive parsing |
| `QueryParameterNames` | Common | `page`, `page-size`, `sort-by`, `sort-order`, `search` — shared by binding and validation |
| `RuleForPage`, `ValidSearch`, `ValidSortField`, `ValidSortOrder` | Common | Reusable FluentValidation rules; errors are reported under the query-parameter name |
| `PageQueryParameters`, `SortQueryParameters` | AspNetCore | Reusable `[FromQuery]` models for controllers |
| `ToPagedResultAsync` | MongoDb / SqlServer | One call to count + page + map for MongoDB collections and EF queries |

Controller → query → store in three lines:

```csharp
public async Task<IActionResult> List([FromQuery] PageQueryParameters paging, [FromQuery] SortQueryParameters sorting, [FromQuery(Name = QueryParameterNames.Search)] string? search, CancellationToken ct)
{
    var result = await sender.Send(new ListCustomersQuery(paging.ToPageRequest(), search, sorting.SortBy, sorting.SortOrder), ct);
    return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
}
```

```csharp
customers.ToPagedResultAsync(filter, sort, criteria.Page, model => model.ToResponse(), cancellationToken);
```

---

## 5. Startup initialization (database-agnostic)

`app.InitializeInfrastructureAsync()` runs every registered `IInfrastructureInitializer` in order, then — in Development only — every `IDataSeeder`:

| Order | Initializer | What it does |
|---|---|---|
| 100 | `SqlServerMigrationInitializer<T>` | Creates the database if missing and applies pending EF migrations (Development, or `SqlServer:ApplyMigrationsOnStartup = true`); retries while SQL Server starts |
| 200 | `MongoIndexInitializer` | Creates collections and indexes (all environments, idempotent) |
| 300 | `KafkaTopicInitializer` | Creates missing topics for published events (Development, or `Kafka:AutoCreateTopics = true`) |
| — | `IDataSeeder` (e.g. `CustomerDataSeeder`) | Seeds data **only when the store is empty**, through the domain model — so seeding also raises events, fills read models and publishes to Kafka |

Startup code never references EF Core, MongoDB or Kafka directly; adding a new store means registering another initializer.

---

## 6. Logging

### 6.1 Sinks

| Sink | Format | Purpose |
|---|---|---|
| Console | Text (default) or compact JSON (`PayNexaLogging:ConsoleFormat = Json`) | Local reading and container log collection |
| Seq | Structured events | Search and analysis (`PayNexaLogging:SeqServerUrl`) |
| Rolling file | Compact JSON, daily, 100 MB max, 7 files retained | Durable fallback when Seq is unreachable (`PayNexaLogging:FileDirectory`) |

### 6.2 Properties on every event

`ServiceName`, `ServiceVersion`, `Environment`, `MachineName`, `ProcessId`, `ThreadId`, `CorrelationId`, `RequestId`, `TraceId`, `SpanId`, `SourceContext`, `EventId`; commands and queries add `Operation`/`OperationKind`, steps add `Step`.

### 6.3 What is logged automatically

| Source | Events |
|---|---|
| Inbound HTTP | `HTTP POST /api/v1/customers started` · `… responded 201 in 83.21 ms` |
| Command / query | `Command CreateCustomerCommand started` · `succeeded` / `failed with Customer.NotFound (NotFound)` · `is slow` · `ended` |
| Validation | `Validation started` · `succeeded` / `failed … Reason: 3 validation error(s)` (field names only) · `ended` |
| Domain events | `Handle domain event CustomerRegisteredDomainEvent started / succeeded / ended` |
| SQL Server | `SQL Server INSERT on CustomerDb started (SaveChanges)` · `succeeded in N ms` / `failed` |
| MongoDB | `MongoDB update on customers started` · `succeeded in N ms` / `failed` |
| Redis | `Redis GET started` · `succeeded` (`CacheKey`, `CacheHit`) · `ended` |
| Outbox | `Dispatch CustomerCreatedIntegrationEvent started / succeeded / failed / ended`, retries, dead-lettering |
| Kafka | `Kafka publish customer.created to paynexa.customer started` · `succeeded` (`Partition`, `Offset`) · `ended` |
| Service-to-service HTTP | `payment-service -> customer-service GetCustomer started / succeeded / failed / ended`, attempts |
| Resilience | retries, circuit OPENED / HALF-OPEN / CLOSED |
| Startup | `Initialize CustomerDbContext migrations`, `Initialize Kafka topics`, `Seed Customers` |

### 6.4 Steps inside a handler

```csharp
using (var step = logger.BeginStep("Email uniqueness check"))
{
    if (await customers.EmailExistsAsync(email, cancellationToken))
    {
        step.Failed(CustomerErrorCodes.EmailAlreadyRegistered);
        return Errors.Customer.EmailAlreadyRegistered;
    }

    step.Succeeded();
}
```

### 6.5 Levels and event ids

| Level | Used for |
|---|---|
| Debug | Health/OpenAPI/Swagger requests, background polling queries |
| Information | Lifecycle: started, succeeded, ended, business events |
| Warning | Expected failures, slow operations, retries, tolerated cache/broker outages |
| Error | Unexpected exceptions, 5xx, downstream unavailable, circuit opened |
| Critical | Dead-lettered outbox messages, process crashes |

| Range | Area |
|---|---|
| 1000 / 1100 | Operations / steps |
| 2000 | Inbound HTTP and correlation |
| 3000 | Service-to-service calls and resilience |
| 4000 / 4100 / 4200 / 4300 | MongoDB / Redis / SQL Server + outbox / Kafka |
| 5000 | Exceptions |
| 10000+ | Service business events |

### 6.6 Sensitive data and correlation

- Properties named like `password`, `secret`, `token`, `apikey`, `authorization`, `credential`, `cardnumber`, `cvv`, `connectionstring`, `privatekey` are redacted; `*Email*` and `*Phone*` are partially masked — including nested objects.
- `X-Correlation-Id` is accepted (validated) or generated, echoed in the response, added to every log event, tagged on traces, stored with outbox messages, sent as a Kafka header and forwarded on outbound HTTP calls.

---

## 7. Errors and exceptions

| `ErrorType` | HTTP |
|---|---|
| Validation | 400 (with field `errors`) |
| Unauthorized / Forbidden | 401 / 403 |
| NotFound | 404 |
| Conflict | 409 |
| BusinessRule | 422 |
| Unavailable | 503 |
| Failure | 500 |

| Exception (global handler) | HTTP | `errorCode` |
|---|---|---|
| `DownstreamServiceException` (timeout) | 504 | `Dependency.Timeout` |
| `DownstreamServiceException` (circuit open / unreachable) | 503 | `Dependency.Unavailable` |
| `BadHttpRequestException` | 400 | `Request.Invalid` |
| Anything else | 500 | `Server.Unexpected` |

All titles and messages come from `CommonErrorMessages`, `ValidationMessages` and each service's `*ErrorMessages`. Responses are RFC 9457 Problem Details with `traceId`, `correlationId` and `errorCode`; stack traces are never returned.

---

## 8. Write store, read store, cache

- Writes go through repositories + `IUnitOfWork` into SQL Server; `EfUnitOfWork` dispatches domain events, then saves, and translates duplicate keys / stale versions into `UniqueConstraintViolationException` / `ConcurrencyConflictException`.
- `OutboxProcessor` claims rows with `UPDLOCK, READPAST`, runs every `IOutboxMessageHandler<T>` (projection, Kafka publisher, …), retries with exponential backoff up to 5 minutes, dead-letters after 20 attempts.
- Projections use `UpsertIfNewerAsync` (version-checked, replay-safe) and then invalidate the Redis entry.
- All `DateTime` values are UTC end to end (`UseUtcDateTimes()`).
- Money is always read from SQL Server (requirements §5.1).

Migrations are code first (`dotnet tool restore`, then `dotnet ef migrations add <Name> --project <Service>.Infrastructure --startup-project <Service>.Infrastructure --output-dir Persistence/Migrations`); remove the generated comment lines.

---

## 9. Service-to-service calls

```csharp
services.AddServiceClient<ICustomerClient, CustomerClient>("customer-service", new Uri("http://customer.api:8080"));
```

Pipeline: `ServiceCallLoggingHandler` → standard resilience (timeouts, retry with jitter for safe methods only, circuit breaker) → `OutboundAttemptHandler` (attempt count, `X-Correlation-Id`) → OpenTelemetry. Failures surface as `DownstreamServiceException` (503/504). Metrics: `paynexa.service_call.duration|failures|retries|circuit_opened`.

---

## 10. Health checks

| Endpoint | Includes |
|---|---|
| `/health/live` | Process only |
| `/health/ready` | SQL Server, MongoDB (unhealthy when down), Redis and Kafka (degraded when down — reads fall back / events wait in the outbox) |
| `/health` | Everything |

---

## 11. Ports, Swagger and Docker

| Service | HTTPS | HTTP | Swagger (Development) |
|---|---|---|---|
| API Gateway | 6000 | 5000 | — (`/health`) |
| Customer | 6001 | 5001 | https://localhost:6001/swagger |
| Authentication | 6002 | 5002 | https://localhost:6002/swagger |
| Payment | 6003 | 5003 | https://localhost:6003/swagger |
| Transaction | 6004 | 5004 | https://localhost:6004/swagger |
| Notification | 6005 | 5005 | https://localhost:6005/swagger |

- The same ports are used by `dotnet run` / F5 (`launchSettings.json`, Swagger opens automatically) and by Docker (containers listen on 8080/8081, mapped to these ports).
- Containers serve HTTPS with the ASP.NET Core development certificate exported to `%APPDATA%\ASP.NET\Https\paynexa-dev.pfx` (password `DEV_CERT_PASSWORD` in `src/.env`):

  ```powershell
  dotnet dev-certs https --trust
  dotnet dev-certs https -ep "$env:APPDATA\ASP.NET\Https\paynexa-dev.pfx" -p <DEV_CERT_PASSWORD>
  ```

---

## 12. Configuration reference

| Key | Default | Notes |
|---|---|---|
| `Service:Name` | — (required) | Log events, traces, Kafka `source-service` header |
| `ConnectionStrings:<Service>Db` | — (required) | Secret — environment/Vault only |
| `MongoDb:ConnectionString` / `DatabaseName` | — (required) | Validated at startup |
| `Redis:ConnectionString` | — (required) | Validated at startup |
| `Kafka:BootstrapServers` | — (required) | `kafka:29092` in Docker, `localhost:9092` on the host |
| `Kafka:AutoCreateTopics` | Development | `TopicPartitions` 3, `TopicReplicationFactor` 1, `MessageTimeoutMs` 10000 |
| `SqlServer:ApplyMigrationsOnStartup` | Development | Other environments migrate from CI/CD |
| `Outbox:PollingInterval` / `BatchSize` / `MaxAttempts` / `MaxRetryDelay` | 1 s / 50 / 20 / 5 min | Validated |
| `OperationLogging:SlowOperationThresholdMs` | 500 | |
| `PayNexaLogging:SeqServerUrl` / `ConsoleFormat` / `FileDirectory` | — / Text / temp | |
| `Observability:OtlpTracesEndpoint` / `OtlpMetricsEndpoint` | — | Seq accepts traces at `/ingest/otlp/v1/traces` |

---

## 13. Adding a new service — checklist

1. Projects `src/Services/<Service>/<Service>.{API,Application,Domain,Infrastructure,Contracts}` with `<RootNamespace>PayNexa.<Services>.<Layer></RootNamespace>`.
2. Domain: `<Name>Aggregate/` with a strongly typed id, value objects, enums, domain events; `Common/Errors/Errors.<Name>.cs` + `*ErrorCodes` + `*ErrorMessages`.
3. Application: commands/queries returning `Result<T>`, one validator each, domain-event handlers that enqueue integration events, `AddApplication()`.
4. Contracts: requests/responses and integration events with `[IntegrationEvent]`.
5. Infrastructure: `DbContext` with `ApplyOutbox()` + `UseUtcDateTimes()`, repositories, read store + projections, seeder, `AddInfrastructure()` composed of write store / read store / caching / messaging.
6. API: `DependencyInjection.cs` (`AddPresentation` / `UsePresentation`), controllers deriving `ApiControllerBase`, `launchSettings.json` on the assigned ports.
7. Docker: Dockerfile, `docker-compose.yml`, `docker-compose.override.yml` (ports, environment, certificate volume), `.env.example`; rebuild and run the containers.
8. Tests in `tests/Unit/<Service>.UnitTests`; document the service in `doc/<Service>.md`.
