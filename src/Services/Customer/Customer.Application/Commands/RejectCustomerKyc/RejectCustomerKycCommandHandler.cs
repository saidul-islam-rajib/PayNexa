using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Commands.RejectCustomerKyc;

public sealed class RejectCustomerKycCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RejectCustomerKycCommandHandler> logger)
    : CustomerCommandHandler<RejectCustomerKycCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, RejectCustomerKycCommand command, DateTime utcNow) =>
        customer.RejectKyc(StatusReason.Create(command.Reason), utcNow);
}
