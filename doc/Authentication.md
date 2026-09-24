# Authentication

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
