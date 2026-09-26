using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Commands.CloseCustomer;

public sealed class CloseCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CloseCustomerCommandHandler> logger)
    : CustomerCommandHandler<CloseCustomerCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, CloseCustomerCommand command, DateTime utcNow) =>
        customer.Close(StatusReason.Create(command.Reason), utcNow);
}
