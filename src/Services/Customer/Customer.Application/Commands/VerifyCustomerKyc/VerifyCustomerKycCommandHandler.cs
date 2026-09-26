using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;

namespace PayNexa.Customers.Application.Commands.VerifyCustomerKyc;

public sealed class VerifyCustomerKycCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<VerifyCustomerKycCommandHandler> logger)
    : CustomerCommandHandler<VerifyCustomerKycCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, VerifyCustomerKycCommand command, DateTime utcNow) =>
        customer.VerifyKyc(utcNow);
}
