using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Domain.CustomerAggregate;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerAddress;

public sealed class ChangeCustomerAddressCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ChangeCustomerAddressCommandHandler> logger)
    : CustomerCommandHandler<ChangeCustomerAddressCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, ChangeCustomerAddressCommand command, DateTime utcNow) =>
        customer.ChangeAddress(command.Address.ToAddress(), utcNow);
}
