using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<CustomerOptions> options,
    ILogger<RegisterCustomerCommandHandler> logger)
    : ICommandHandler<RegisterCustomerCommand, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        using (var step = logger.BeginStep("Email uniqueness check"))
        {
            if (await customers.EmailExistsAsync(email, cancellationToken))
            {
                step.Failed(CustomerErrorCodes.EmailAlreadyRegistered);
                return Errors.Customer.EmailAlreadyRegistered;
            }

            step.Succeeded();
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var customer = Customer.Register(
            PersonName.Create(command.FirstName, command.LastName),
            email,
            PhoneNumber.Create(command.PhoneNumber),
            command.DateOfBirth,
            command.Address?.ToAddress(),
            options.Value.ToPolicy(),
            utcNow);

        customers.Add(customer);

        using (var step = logger.BeginStep("Persist customer to write store"))
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                step.WithProperty("CustomerId", customer.Id.Value).Succeeded();
            }
            catch (UniqueConstraintViolationException)
            {
                step.Failed(CustomerErrorCodes.EmailAlreadyRegistered);
                return Errors.Customer.EmailAlreadyRegistered;
            }
        }

        CustomerLog.Registered(logger, customer.Id.Value);
        return customer.ToResponse();
    }
}
