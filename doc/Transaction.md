# Transaction Service

**Status:** Planned — not implemented yet
**Service name:** `transaction-service` · **Container:** `transaction.api` · **Ports:** HTTPS `6004`, HTTP `5004`

Maintains the financial transaction ledger derived from payment outcomes.

## 1. Responsibilities

- Consume `payment.completed` and `payment.failed` from Kafka with an idempotent consumer (duplicate events are ignored, requirements §16, §46).
- Record transactions (`CreateTransactionCommand`) with strong transactional boundaries.
- Serve transaction lookups and a customer's transaction history.

## 2. Data ownership (requirements §5.1)

| Store | Role | Contents |
|---|---|---|
| SQL Server `TransactionDb` | Write store **and the read store for money** | Transactions, amounts, balances, processed-event ids, outbox |
| MongoDB `paynexa_transaction` | Read store for non-financial views | History views without monetary values |
| Redis | Short-lived state | `idempotency:transaction:{eventId}` — never amounts |

**Money rule:** every query that returns an amount or balance reads SQL Server. MongoDB projections of this service never hold monetary values.

## 3. Planned API

| Method | Route |
|---|---|
| `GET` | `/api/v1/transactions/{id}` |
| `GET` | `/api/v1/transactions?customer-id=…&page=…&page-size=…` |
| `GET` | `/api/v1/transactions/{id}/audit-trail` |
