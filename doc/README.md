# PayNexa Documentation

| Document | Contents |
|---|---|
| [fintech_microservices_industry_requirements.md](fintech_microservices_industry_requirements.md) | The platform requirements |
| [BuildingBlocks.md](BuildingBlocks.md) | Shared foundation: DDD base types, events and Kafka, pagination, startup initialization, logging, errors, stores, service clients, ports, configuration |
| [ApiGateway.md](ApiGateway.md) | YARP gateway |
| [Authentication.md](Authentication.md) | Authentication Service |
| [Customer.md](Customer.md) | Customer Service |
| [Payment.md](Payment.md) | Payment Service |
| [Transaction.md](Transaction.md) | Transaction Service |
| [Notification.md](Notification.md) | Notification Service |

## Services

| Service | Status | HTTPS | HTTP | Swagger (Development) |
|---|---|---|---|---|
| API Gateway | Routing (YARP), logging, correlation, health | 6000 | 5000 | — |
| Customer | **Implemented** | 6001 | 5001 | https://localhost:6001/swagger |
| Authentication | Planned | 6002 | 5002 | https://localhost:6002/swagger |
| Payment | Planned | 6003 | 5003 | https://localhost:6003/swagger |
| Transaction | Planned | 6004 | 5004 | https://localhost:6004/swagger |
| Notification | Planned | 6005 | 5005 | https://localhost:6005/swagger |

The same ports apply to `dotnet run` / F5 and to Docker. Swagger opens automatically when a service starts in Development.

## Data rules at a glance

- Commands write to **SQL Server** (source of truth); queries read **MongoDB** read models projected through the transactional outbox.
- **Money is always read from SQL Server** — never from MongoDB or Redis.
- Redis caches read models only.
- Aggregates raise domain events; integration events are published to **Kafka** from the outbox.

## Local environment

| Component | Address |
|---|---|
| Seq (log search) | http://localhost:5342 — user `admin`, password `SEQ_ADMIN_PASSWORD` from `src/.env` |
| Kafka UI | http://localhost:8090 |
| Kafka | `localhost:9092` (host) · `kafka:29092` (containers) |
| SQL Server | `localhost,1433` — user `sa`, password `SQL_SA_PASSWORD` from `src/.env` |
| MongoDB | `mongodb://localhost:27017` |
| Redis | `localhost:6379` |

### First-time setup

1. Copy `src/.env.example` to `src/.env` and set strong passwords (the file is gitignored).
2. Trust and export the development certificate used for HTTPS inside containers:

   ```powershell
   dotnet dev-certs https --trust
   dotnet dev-certs https -ep "$env:APPDATA\ASP.NET\Https\paynexa-dev.pfx" -p <DEV_CERT_PASSWORD from src/.env>
   ```

3. `dotnet tool restore` (pinned `dotnet-ef`).
4. Store the SQL password for local runs (connection strings contain no password):

   ```powershell
   dotnet user-secrets set "SqlServer:Password" "<SQL_SA_PASSWORD>" --project src/Services/Customer/Customer.API
   ```

### Run

- **Visual Studio:** open `src/PayNexa.slnx`, set `docker-compose` as the startup project, press F5 — Customer Swagger opens.
- **Command line (from the repository root):** `docker compose up -d --build` builds every image and starts everything; `docker compose down` stops it (data is kept). Rebuild one service with `docker compose up -d --build customer.api`. The root `compose.yaml` includes `src/docker-compose.yml` + `src/docker-compose.override.yml` and reads `src/.env`.

Everything runs as one Docker Compose project, `paynexa`; container names start with their category so they are grouped in Docker Desktop:

| Prefix | Containers |
|---|---|
| `database-` | `database-sqlserver`, `database-mongodb`, `database-redis` |
| `infrastructure-` | `infrastructure-kafka`, `infrastructure-kafka-ui`, `infrastructure-seq` |
| `service-` | `service-apigateway`, `service-customer-api`, `service-authentication-api`, `service-payment-api`, `service-transaction-api`, `service-notification-api` |

Compose commands use the service names (`sqlserver`, `customer.api`, …); `docker exec` / `docker logs` use the container names (`database-sqlserver`, `service-customer-api`, …). Containers reach each other by service name.

In Development every service creates its databases, collections, indexes and Kafka topics on startup, and seeds data when its store is empty.

### Run APIs locally with local SQL Server and MongoDB

The APIs run from Visual Studio or `dotnet run`, SQL Server and MongoDB are installed on the machine, and Redis, Kafka, Kafka UI and Seq stay in Docker.

1. Free the ports and start only the shared infrastructure:

   ```powershell
   docker compose stop sqlserver mongodb paynexa.apigateway customer.api authentication.api payment.api transaction.api notification.api
   docker compose up -d redis kafka kafka-ui seq
   ```

2. Point each API at the local SQL Server with Windows Authentication (machine-specific, so it lives in user-secrets, never in `appsettings`). Example for LocalDB:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:CustomerDb" "Server=(localdb)\MSSQLLocalDB;Database=CustomerDb;Trusted_Connection=True;TrustServerCertificate=True" --project src\Services\Customer\Customer.API
   dotnet user-secrets remove "SqlServer:Password" --project src\Services\Customer\Customer.API
   ```

   Connection string names: `AuthDb`, `CustomerDb`, `PaymentDb`, `TransactionDb`, `NotificationDb`. `SqlServer:Password` is only merged in when set, so Windows Authentication needs it removed.

3. Run a local MongoDB on `localhost:27017` (default install, no settings needed).
4. Start the APIs with the `https` launch profile (`dotnet run --project src\Services\Customer\Customer.API --launch-profile https`, or multiple startup projects in Visual Studio). Databases are created, migrated and seeded on first start.

### Test

```powershell
dotnet test --solution src/PayNexa.slnx
```
