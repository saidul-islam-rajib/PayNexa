using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayNexa.AspNetCore.Controllers;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Commands.CreateCustomer;
using PayNexa.Customers.Application.Commands.UpdateCustomer;
using PayNexa.Customers.Application.Queries.GetCustomerById;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Requests;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomersController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.DateOfBirth);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id, version = RequestedVersion() }, result.Value)
            : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<CustomerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        [FromQuery(Name = "search")] string? search = null,
        [FromQuery(Name = "sort-by")] string? sortBy = null,
        [FromQuery(Name = "sort-order")] string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListCustomersQuery(page, pageSize, search, sortBy, sortOrder), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(id, request.FirstName, request.LastName, request.PhoneNumber);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    private string RequestedVersion() => RouteData.Values["version"]?.ToString() ?? "1";
}
