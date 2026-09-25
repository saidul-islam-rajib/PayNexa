using Mediator;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Errors;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.DomainExceptions;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Application.Commands.CreateCustomer;

public sealed partial class CreateCustomerCommandHandler(
    ICustomerRepository customers,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CreateCustomerCommandHandler> logger)
    : ICommandHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        Customer customer;
        try
        {
            customer = Customer.Create(
                command.FirstName,
                command.LastName,
                Email.Create(command.Email),
                PhoneNumber.Create(command.PhoneNumber),
                command.DateOfBirth,
                timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException exception)
        {
            return Error.BusinessRule(exception.Code, exception.Message);
        }

        using (var step = logger.BeginStep("Email uniqueness check"))
        {
            if (await customers.EmailExistsAsync(customer.Email, cancellationToken))
            {
                step.Failed("Email already registered");
                return CustomerErrors.EmailAlreadyRegistered;
            }

            step.Succeeded();
        }

        customers.Add(customer);
        outbox.Enqueue(customer.ToCreatedEvent(customer.CreatedAtUtc));

        using (var step = logger.BeginStep("Persist customer to write store"))
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                step.WithProperty("CustomerId", customer.Id).Succeeded();
            }
            catch (UniqueConstraintViolationException)
            {
                step.Failed("Email registered by a concurrent request");
                return CustomerErrors.EmailAlreadyRegistered;
            }
        }

        LogCustomerCreated(logger, customer.Id);
        return customer.ToResponse();
    }

    [LoggerMessage(EventId = 10001, Level = LogLevel.Information, Message = "Customer {CustomerId} created")]
    private static partial void LogCustomerCreated(ILogger logger, Guid customerId);
}
