using Mediator;
using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Controllers;
using PayNexa.Common.Querying;
using PayNexa.Customers.API.Mappings;
using PayNexa.Customers.API.Requests;
using PayNexa.Customers.Application.Commands.ReactivateCustomer;
using PayNexa.Customers.Application.Commands.VerifyCustomerKyc;
using PayNexa.Customers.Application.Queries.GetCustomerByEmail;
using PayNexa.Customers.Application.Queries.GetCustomerById;
using PayNexa.Customers.Application.Queries.GetCustomerPaymentEligibility;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Requests;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers.V1;

public sealed class CustomersController(ISender sender) : ApiControllerBase
{
    private const string ById = "{id:guid}";

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Register(RegisterCustomerRequest request, CancellationToken cancellationToken) =>
        RespondCreated(await sender.Send(request.ToCommand(), cancellationToken), nameof(GetById), customer => new { id = customer.Id });

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerResponse>>> List([FromQuery] ListCustomersRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToQuery(), cancellationToken));

    [HttpGet(ById)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Respond(await sender.Send(new GetCustomerByIdQuery(id), cancellationToken));

    [HttpGet("[action]")]
    public async Task<ActionResult<CustomerResponse>> Lookup([FromQuery(Name = CustomerQueryParameterNames.Email)] string email, CancellationToken cancellationToken) =>
        Respond(await sender.Send(new GetCustomerByEmailQuery(email), cancellationToken));

    [HttpGet($"{ById}/[action]")]
    public async Task<ActionResult<PaymentEligibilityResponse>> PaymentEligibility(Guid id, CancellationToken cancellationToken) =>
        Respond(await sender.Send(new GetCustomerPaymentEligibilityQuery(id), cancellationToken));

    [HttpPut(ById)]
    public async Task<ActionResult<CustomerResponse>> UpdateProfile(Guid id, UpdateCustomerProfileRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPut($"{ById}/email")]
    public async Task<ActionResult<CustomerResponse>> ChangeEmail(Guid id, ChangeCustomerEmailRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPut($"{ById}/address")]
    public async Task<ActionResult<CustomerResponse>> ChangeAddress(Guid id, ChangeCustomerAddressRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToCommand(id), cancellationToken));

    [HttpPost($"{ById}/[action]")]
    public async Task<ActionResult<CustomerResponse>> Suspend(Guid id, CustomerStatusChangeRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToSuspendCommand(id), cancellationToken));

    [HttpPost($"{ById}/[action]")]
    public async Task<ActionResult<CustomerResponse>> Reactivate(Guid id, CancellationToken cancellationToken) =>
        Respond(await sender.Send(new ReactivateCustomerCommand(id), cancellationToken));

    [HttpPost($"{ById}/[action]")]
    public async Task<ActionResult<CustomerResponse>> Close(Guid id, CustomerStatusChangeRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToCloseCommand(id), cancellationToken));

    [HttpPost($"{ById}/kyc/verify")]
    public async Task<ActionResult<CustomerResponse>> VerifyKyc(Guid id, CancellationToken cancellationToken) =>
        Respond(await sender.Send(new VerifyCustomerKycCommand(id), cancellationToken));

    [HttpPost($"{ById}/kyc/reject")]
    public async Task<ActionResult<CustomerResponse>> RejectKyc(Guid id, CustomerStatusChangeRequest request, CancellationToken cancellationToken) =>
        Respond(await sender.Send(request.ToRejectKycCommand(id), cancellationToken));
}
