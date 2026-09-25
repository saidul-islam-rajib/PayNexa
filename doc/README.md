# PayNexa Documentation

| Document | Contents |
|---|---|
| [fintech_microservices_industry_requirements.md](fintech_microservices_industry_requirements.md) | The platform requirements |
| [BuildingBlocks.md](BuildingBlocks.md) | Shared foundation: logging, errors, write/read stores, outbox, caching, service clients, health, configuration |
| [ApiGateway.md](ApiGateway.md) | YARP gateway |
| [Authentication.md](Authentication.md) | Authentication Service |
| [Customer.md](Customer.md) | Customer Service |
| [Payment.md](Payment.md) | Payment Service |
| [Transaction.md](Transaction.md) | Transaction Service |
| [Notification.md](Notification.md) | Notification Service |

## Service status

| Service | Status | Port (compose) |
|---|---|---|
| API Gateway | Skeleton (logging, correlation, health) | 8000 |
| Authentication | Planned | 8001 |
| Customer | **Implemented** | 8002 |
| Payment | Planned | 8003 |
| Transaction | Planned | 8004 |
| Notification | Planned | 8005 |

## Data rules at a glance

- Commands write to **SQL Server** (source of truth); queries read **MongoDB** read models projected through the transactional outbox.
- **Money is always read from SQL Server** — never from MongoDB or Redis.
- Redis caches read models only.

## Local environment

| Component | Address |
|---|---|
| Seq (log search UI) | http://localhost:5342 — user `admin`, password `SEQ_ADMIN_PASSWORD` from `src/.env` |
| Seq ingestion / OTLP traces | http://localhost:5341 |
| SQL Server | `localhost,1433` — user `sa`, password `SQL_SA_PASSWORD` from `src/.env` |
| MongoDB | `mongodb://localhost:27017` |
| Redis | `localhost:6379` |

1. Copy `src/.env.example` to `src/.env` and set strong passwords (the file is gitignored).
2. In Visual Studio, open `src/PayNexa.slnx`, set `docker-compose` as the startup project and press F5 — or run `docker compose up -d` from `src/`.
3. Run `dotnet tool restore` once to get the pinned `dotnet-ef`.
4. Run all tests with `dotnet test --solution src/PayNexa.slnx`.
