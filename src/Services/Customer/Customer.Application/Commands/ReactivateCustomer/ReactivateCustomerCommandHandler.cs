using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;

namespace PayNexa.Customers.Application.Commands.ReactivateCustomer;

public sealed class ReactivateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ReactivateCustomerCommandHandler> logger)
    : CustomerCommandHandler<ReactivateCustomerCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, ReactivateCustomerCommand command, DateTime utcNow) =>
        customer.Reactivate(utcNow);
}
