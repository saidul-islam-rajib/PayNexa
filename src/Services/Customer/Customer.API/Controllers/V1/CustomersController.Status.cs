using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Controllers;
using PayNexa.Customers.API.Mappings;
using PayNexa.Customers.Application.Commands.ReactivateCustomer;
using PayNexa.Customers.Contracts.Requests;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers.V1;

public sealed partial class CustomersController
{
    [HttpPost(ApiRoutes.ByIdAction)]
    public Task<ActionResult<CustomerResponse>> Suspend(
        Guid id,
        CustomerStatusChangeRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToSuspendCommand(id), cancellationToken);

    [HttpPost(ApiRoutes.ByIdAction)]
    public Task<ActionResult<CustomerResponse>> Reactivate(Guid id, CancellationToken cancellationToken) =>
        CommandAsync(new ReactivateCustomerCommand(id), cancellationToken);

    [HttpPost(ApiRoutes.ByIdAction)]
    public Task<ActionResult<CustomerResponse>> Close(
        Guid id,
        CustomerStatusChangeRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToCloseCommand(id), cancellationToken);
}
