using Mediator;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Commands.Common;

public abstract class CustomerCommandHandler<TCommand>(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger logger)
    : ICommandHandler<TCommand, Result<CustomerResponse>>
    where TCommand : ICustomerCommand
{
    private const string PersistStep = "Persist customer to write store";

    protected ICustomerRepository Customers => customers;

    public async ValueTask<Result<CustomerResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(CustomerId.Create(command.CustomerId), cancellationToken);

        if (customer is null)
        {
            return Errors.Customer.NotFound(command.CustomerId);
        }

        if (await CheckPreconditionsAsync(customer, command, cancellationToken) is { } precondition)
        {
            return precondition;
        }

        Apply(customer, command, timeProvider.GetUtcNow().UtcDateTime);

        using (var step = logger.BeginStep(PersistStep))
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                step.WithProperty("CustomerVersion", customer.Version).Succeeded();
            }
            catch (ConcurrencyConflictException)
            {
                step.Failed(CustomerErrorCodes.ConcurrentModification);
                return Errors.Customer.ConcurrentModification;
            }
            catch (UniqueConstraintViolationException)
            {
                step.Failed(CustomerErrorCodes.EmailAlreadyRegistered);
                return Errors.Customer.EmailAlreadyRegistered;
            }
        }

        CustomerLog.Changed(logger, typeof(TCommand).Name, customer.Id.Value, customer.Version);
        return customer.ToResponse();
    }

    protected virtual ValueTask<Error?> CheckPreconditionsAsync(Customer customer, TCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromResult<Error?>(null);

    protected abstract void Apply(Customer customer, TCommand command, DateTime utcNow);
}
