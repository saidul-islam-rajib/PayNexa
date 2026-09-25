# Authentication Service

**Status:** Planned — not implemented yet
**Service name:** `authentication-service` · **Container:** `authentication.api` · **Port (compose):** `8001`

Issues and manages identities and tokens for the platform (requirements §12).

## 1. Responsibilities

- Registration, login, password hashing.
- JWT access tokens and rotating refresh tokens; token revocation.
- Roles, permissions and policy data consumed by the gateway and services.
- Security events (failed logins, revocations) logged separately and searchable (requirements §30).

JWT signing keys come from HashiCorp Vault — never from appsettings, Git or images (requirements §12, §13).

## 2. Data ownership (requirements §5.1)

| Store | Role | Contents |
|---|---|---|
| SQL Server `AuthDb` | Write store, source of truth | Users, password hashes, refresh tokens (hashed), revocations, roles, outbox |
| MongoDB `paynexa_auth` | Read store | User profile read model |
| Redis | Cache / short-lived state | Revoked-token list, login rate limiting (`ratelimit:auth:{clientId}`) |

Passwords, password hashes, access tokens and refresh tokens are never logged; the masking enricher redacts them automatically (see [BuildingBlocks.md](BuildingBlocks.md#27-sensitive-data-requirements-44)).

## 3. Planned API

Base path: `/api/v1/auth`.


```js
POST {{host}}/api/v1/auth/register
```
### Register Request
```json
{
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"password":"Test@123"
}
```

### Register Response
```json
{
	"userId":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"accessToken":"bearertoken....",
	"accessTokenExpiresAtUtc":"2026-09-25T13:00:00Z",
	"refreshToken":"refreshtoken...."
}
```


```js
POST {{host}}/api/v1/auth/login
```
### Login Request
```json
{
	"email":"saidul.is.rajib@gmail.com",
	"password":"Test@123"
}
```

### Login Response
```json
{
	"userId":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"accessToken":"bearertoken....",
	"accessTokenExpiresAtUtc":"2026-09-25T13:00:00Z",
	"refreshToken":"refreshtoken...."
}
```


```js
POST {{host}}/api/v1/auth/refresh-token
```
### Refresh Token Request
```json
{
	"refreshToken":"refreshtoken...."
}
```

### Refresh Token Response
```json
{
	"userId":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"accessToken":"bearertoken....",
	"accessTokenExpiresAtUtc":"2026-09-25T13:00:00Z",
	"refreshToken":"refreshtoken...."
}
```


```js
POST {{host}}/api/v1/auth/revoke-token
```
### Revoke Token Request
```json
{
	"refreshToken":"refreshtoken...."
}
```

### Revoke Token Response
```
204 No Content
```
