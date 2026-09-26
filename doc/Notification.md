# Notification Service

**Status:** Planned — not implemented yet
**Service name:** `notification-service` · **Compose service:** `notification.api` · **Container:** `service-notification-api` · **Ports:** HTTPS `6005`, HTTP `5005`

Sends email/SMS notifications in reaction to platform events. It has no public gateway route — it is reachable only through Kafka (requirements §3.2).

## 1. Responsibilities

- Consume `payment.completed` and `payment.failed` and send notifications (`SendNotificationCommand`).
- Track delivery state, retries and failures; route poison messages to a dead-letter topic (requirements §46).
- Expose health endpoints only.

## 2. Data ownership (requirements §5.1)

| Store | Role | Contents |
|---|---|---|
| SQL Server `NotificationDb` | Write store, source of truth | Notification requests, delivery attempts and state, processed-event ids, outbox |
| MongoDB `paynexa_notification` | Read store | Notification history read model |
| Redis | Short-lived state | `idempotency:notification:{eventId}` |

Email/SMS provider credentials come from Vault. Recipient addresses and phone numbers are masked in logs.
