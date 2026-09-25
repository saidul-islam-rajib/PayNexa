using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Controllers;
using PayNexa.Customers.API.Mappings;
using PayNexa.Customers.API.Routing;
using PayNexa.Customers.Contracts.Requests;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers.V1;

public sealed partial class CustomersController
{
    [HttpPost]
    public Task<ActionResult<CustomerResponse>> Register(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken) =>
        CreateAsync(request.ToCommand(), nameof(GetById), customer => customer.Id, cancellationToken);

    [HttpPut(ApiRoutes.ById)]
    public Task<ActionResult<CustomerResponse>> UpdateProfile(
        Guid id,
        UpdateCustomerProfileRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToCommand(id), cancellationToken);

    [HttpPut(CustomerRoutes.Email)]
    public Task<ActionResult<CustomerResponse>> ChangeEmail(
        Guid id,
        ChangeCustomerEmailRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToCommand(id), cancellationToken);

    [HttpPut(CustomerRoutes.Address)]
    public Task<ActionResult<CustomerResponse>> ChangeAddress(
        Guid id,
        ChangeCustomerAddressRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToCommand(id), cancellationToken);
}
