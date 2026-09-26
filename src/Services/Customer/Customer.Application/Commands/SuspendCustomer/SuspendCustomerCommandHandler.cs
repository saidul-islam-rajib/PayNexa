using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Commands.SuspendCustomer;

public sealed class SuspendCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<SuspendCustomerCommandHandler> logger)
    : CustomerCommandHandler<SuspendCustomerCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, SuspendCustomerCommand command, DateTime utcNow) =>
        customer.Suspend(StatusReason.Create(command.Reason), utcNow);
}
