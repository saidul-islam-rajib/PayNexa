# Payment Service

**Status:** Planned — not implemented yet
**Service name:** `payment-service` · **Container:** `payment.api` · **Port (compose):** `8003`

Accepts, validates and processes payments. It is the platform's primary demonstration flow (requirements §40).

## 1. Responsibilities

- Create, process and cancel payments (`CreatePaymentCommand`, `ProcessPaymentCommand`, `CancelPaymentCommand`).
- Validate the customer synchronously against Customer Service (REST) before accepting a payment.
- Enforce idempotency for every payment request (`Idempotency-Key`, requirements §16).
- Publish `payment.completed` and `payment.failed` through the transactional outbox (requirements §15).

## 2. Data ownership (requirements §5.1)

| Store | Role | Contents |
|---|---|---|
| SQL Server `PaymentDb` | Write store **and the read store for money** | Payments, amounts, currency, status history, idempotency records, outbox |
| MongoDB `paynexa_payment` | Read store for non-financial data only | Payment metadata, provider references |
| Redis | Cache | `payment:{paymentId}:status`, `idempotency:payment:{key}` — never amounts |

**Money rule:** amounts and every decision based on an amount are read from SQL Server — never from MongoDB or Redis. Payment queries that return amounts read from SQL Server.

## 3. Customer Service dependency

Payment calls Customer Service through a typed client registered with `AddServiceClient<ICustomerClient, CustomerClient>("customer-service", …)` (see [BuildingBlocks.md](BuildingBlocks.md#5-service-to-service-calls-requirements-19-26)):

- `GET /api/v1/customers/{id}` is retried with backoff; the circuit opens when Customer Service keeps failing.
- An open circuit or timeout returns `503`/`504` Problem Details immediately and is logged at Error with `TargetService = customer-service`.
- The payment itself (POST) is never retried automatically; retries rely on the idempotency key.
- Payment defines its own small customer DTO for this call instead of referencing `Customer.Contracts`, so the services stay free of compile-time coupling.

## 4. Planned API

| Method | Route |
|---|---|
| `POST` | `/api/v1/payments` (requires `Idempotency-Key`) |
| `GET` | `/api/v1/payments/{id}` |
| `POST` | `/api/v1/payments/{id}/cancel` |
| `GET` | `/api/v1/payments?customer-id=…&page=…&page-size=…` |
