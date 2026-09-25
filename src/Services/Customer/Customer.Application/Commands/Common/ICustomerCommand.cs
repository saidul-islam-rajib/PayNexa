using Mediator;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Commands.Common;

public interface ICustomerCommand : ICommand<Result<CustomerResponse>>
{
    Guid CustomerId { get; }
}
