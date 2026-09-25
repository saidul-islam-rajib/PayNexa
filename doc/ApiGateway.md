# API Gateway

**Status:** Routing implemented (YARP); authentication and rate limiting planned
**Service name:** `api-gateway` · **Container:** `paynexa.apigateway` · **Ports:** HTTPS `6000`, HTTP `5000`

Single entry point for clients, built on YARP (requirements §11). It contains no business logic.

## 1. Implemented

- YARP routes from configuration (`ReverseProxy:Routes`/`Clusters`): `/api/v1/auth/**` → Authentication, `/api/v1/customers/**` → Customer, `/api/v1/payments/**` → Payment, `/api/v1/transactions/**` → Transaction. Development targets `localhost:5001–5004`; Docker targets the containers.
- Serilog logging to Console + Seq + file, with the same request lifecycle logs as every service.
- Correlation ID: generated at the gateway when the client does not send `X-Correlation-Id`, and propagated downstream.
- OpenTelemetry tracing and `/health`, `/health/live`, `/health/ready`.

## 2. Planned

- JWT validation and authorization policies.
- Rate limiting backed by Redis (`ratelimit:gateway:{clientId}`), strictest on login and payment creation (requirements §48).
- Request size limits, secure headers and CORS (requirements §30, §49).
- Health-aware routing and gateway-level Problem Details.
