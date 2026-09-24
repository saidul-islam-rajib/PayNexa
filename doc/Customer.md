# Customer

```js
POST {{host}}/api/v1/customers
```
### Create Customer Request
```json
{
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"phoneNumber":"+8801700000000",
	"dateOfBirth":"1995-05-20"
}
```

### Create Customer Response
```json
{
	"id":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"phoneNumber":"+8801700000000",
	"dateOfBirth":"1995-05-20",
	"status":"Active",
	"createdAtUtc":"2026-09-25T13:00:00Z",
	"updatedAtUtc":"2026-09-25T13:00:00Z"
}
```


```js
GET {{host}}/api/v1/customers/{id}
```
### Get Customer By Id Response
```json
{
	"id":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"phoneNumber":"+8801700000000",
	"dateOfBirth":"1995-05-20",
	"status":"Active",
	"createdAtUtc":"2026-09-25T13:00:00Z",
	"updatedAtUtc":"2026-09-25T13:00:00Z"
}
```


```js
GET {{host}}/api/v1/customers?page=1&pageSize=20&search=&sortBy=createdAtUtc
```
### List Customers Response
```json
{
	"items":[
		{
			"id":"00000000-0000-0000-0000-000000000000",
			"firstName":"Saidul Islam",
			"lastName":"Rajib",
			"email":"saidul.is.rajib@gmail.com",
			"phoneNumber":"+8801700000000",
			"dateOfBirth":"1995-05-20",
			"status":"Active",
			"createdAtUtc":"2026-09-25T13:00:00Z",
			"updatedAtUtc":"2026-09-25T13:00:00Z"
		}
	],
	"page":1,
	"pageSize":20,
	"totalCount":1,
	"totalPages":1
}
```


```js
PUT {{host}}/api/v1/customers/{id}
```
### Update Customer Request
```json
{
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"phoneNumber":"+8801700000000"
}
```

### Update Customer Response
```json
{
	"id":"00000000-0000-0000-0000-000000000000",
	"firstName":"Saidul Islam",
	"lastName":"Rajib",
	"email":"saidul.is.rajib@gmail.com",
	"phoneNumber":"+8801700000000",
	"dateOfBirth":"1995-05-20",
	"status":"Active",
	"createdAtUtc":"2026-09-25T13:00:00Z",
	"updatedAtUtc":"2026-09-25T13:00:00Z"
}
```
