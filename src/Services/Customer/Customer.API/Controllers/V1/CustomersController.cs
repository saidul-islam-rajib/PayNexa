using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Controllers;
using PayNexa.Common.Querying;
using PayNexa.Customers.API.Mappings;
using PayNexa.Customers.API.Requests;
using PayNexa.Customers.Application.Queries.GetCustomerById;
using PayNexa.Customers.Application.Queries.GetCustomerPaymentEligibility;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers.V1;

public sealed partial class CustomersController : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<PagedResult<CustomerResponse>>> List(
        [FromQuery] ListCustomersRequest request,
        CancellationToken cancellationToken) =>
        QueryAsync(request.ToQuery(), cancellationToken);

    [HttpGet(ApiRoutes.ById)]
    public Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        QueryAsync(new GetCustomerByIdQuery(id), cancellationToken);

    [HttpGet(ApiRoutes.Action)]
    public Task<ActionResult<CustomerResponse>> Lookup(
        [FromQuery] CustomerLookupRequest request,
        CancellationToken cancellationToken) =>
        QueryAsync(request.ToQuery(), cancellationToken);

    [HttpGet(ApiRoutes.ByIdAction)]
    public Task<ActionResult<PaymentEligibilityResponse>> PaymentEligibility(Guid id, CancellationToken cancellationToken) =>
        QueryAsync(new GetCustomerPaymentEligibilityQuery(id), cancellationToken);
}
