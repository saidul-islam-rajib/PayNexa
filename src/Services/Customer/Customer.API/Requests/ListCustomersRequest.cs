using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Querying;
using PayNexa.Customers.Application.Queries.ListCustomers;

namespace PayNexa.Customers.API.Requests;

public sealed class ListCustomersRequest : PagedRequest
{
    [FromQuery(Name = CustomerQueryParameterNames.Status)]
    public string? Status { get; init; }

    [FromQuery(Name = CustomerQueryParameterNames.KycStatus)]
    public string? KycStatus { get; init; }
}
