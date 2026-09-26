using Microsoft.AspNetCore.Mvc;
using PayNexa.Customers.API.Mappings;
using PayNexa.Customers.API.Routing;
using PayNexa.Customers.Application.Commands.VerifyCustomerKyc;
using PayNexa.Customers.Contracts.Requests;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers.V1;

public sealed partial class CustomersController
{
    [HttpPost(CustomerRoutes.KycAction)]
    [ActionName(CustomerRoutes.KycActions.Verify)]
    public Task<ActionResult<CustomerResponse>> VerifyKyc(Guid id, CancellationToken cancellationToken) =>
        CommandAsync(new VerifyCustomerKycCommand(id), cancellationToken);

    [HttpPost(CustomerRoutes.KycAction)]
    [ActionName(CustomerRoutes.KycActions.Reject)]
    public Task<ActionResult<CustomerResponse>> RejectKyc(
        Guid id,
        CustomerStatusChangeRequest request,
        CancellationToken cancellationToken) =>
        CommandAsync(request.ToRejectKycCommand(id), cancellationToken);
}
