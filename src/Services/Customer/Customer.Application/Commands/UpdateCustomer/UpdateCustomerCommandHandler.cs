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
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Application.Commands.UpdateCustomer;

public sealed partial class UpdateCustomerCommandHandler(
    ICustomerRepository customers,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<UpdateCustomerCommandHandler> logger)
    : ICommandHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(command.Id, cancellationToken);

        if (customer is null)
        {
            return CustomerErrors.NotFound(command.Id);
        }

        try
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            customer.UpdateProfile(command.FirstName, command.LastName, PhoneNumber.Create(command.PhoneNumber), now);
            outbox.Enqueue(customer.ToUpdatedEvent(now));
        }
        catch (DomainException exception)
        {
            return Error.BusinessRule(exception.Code, exception.Message);
        }

        using (var step = logger.BeginStep("Persist customer to write store"))
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                step.WithProperty("CustomerVersion", customer.Version).Succeeded();
            }
            catch (ConcurrencyConflictException)
            {
                step.Failed("Customer modified by a concurrent request");
                return CustomerErrors.ConcurrentModification;
            }
        }

        LogCustomerUpdated(logger, customer.Id, customer.Version);
        return customer.ToResponse();
    }

    [LoggerMessage(EventId = 10002, Level = LogLevel.Information, Message = "Customer {CustomerId} updated to version {CustomerVersion}")]
    private static partial void LogCustomerUpdated(ILogger logger, Guid customerId, long customerVersion);
}
