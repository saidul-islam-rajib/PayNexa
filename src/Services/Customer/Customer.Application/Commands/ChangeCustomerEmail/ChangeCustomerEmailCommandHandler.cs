using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerEmail;

public sealed class ChangeCustomerEmailCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ChangeCustomerEmailCommandHandler> logger)
    : CustomerCommandHandler<ChangeCustomerEmailCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override async ValueTask<Error?> CheckPreconditionsAsync(Customer customer, ChangeCustomerEmailCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        return email != customer.Email && await Customers.EmailExistsAsync(email, cancellationToken)
            ? Errors.Customer.EmailAlreadyRegistered
            : null;
    }

    protected override void Apply(Customer customer, ChangeCustomerEmailCommand command, DateTime utcNow) =>
        customer.ChangeEmail(Email.Create(command.Email), utcNow);
}
