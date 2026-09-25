# API Gateway

**Status:** Skeleton — logging, correlation, tracing and health are wired; routing is not implemented yet
**Service name:** `api-gateway` · **Container:** `paynexa.apigateway` · **Port (compose):** `8000`

Single entry point for clients, built on YARP (requirements §11). It contains no business logic.

## 1. Implemented

- Serilog logging to Console + Seq + file, with the same request lifecycle logs as every service.
- Correlation ID: generated at the gateway when the client does not send `X-Correlation-Id`, and propagated downstream.
- OpenTelemetry tracing and `/health`, `/health/live`, `/health/ready`.

## 2. Planned

| Route | Target |
|---|---|
| `/api/v1/auth/*` | Authentication Service |
| `/api/v1/customers/*` | Customer Service |
| `/api/v1/payments/*` | Payment Service |
| `/api/v1/transactions/*` | Transaction Service |

- JWT validation and authorization policies.
- Rate limiting backed by Redis (`ratelimit:gateway:{clientId}`), strictest on login and payment creation (requirements §48).
- Request size limits, secure headers and CORS (requirements §30, §49).
- Health-aware routing and gateway-level Problem Details.
