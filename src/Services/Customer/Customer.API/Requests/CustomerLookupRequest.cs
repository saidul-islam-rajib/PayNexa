using Microsoft.AspNetCore.Mvc;
using PayNexa.Customers.Application.Queries.ListCustomers;

namespace PayNexa.Customers.API.Requests;

public sealed class CustomerLookupRequest
{
    [FromQuery(Name = CustomerQueryParameterNames.Email)]
    public string Email { get; init; } = string.Empty;
}
