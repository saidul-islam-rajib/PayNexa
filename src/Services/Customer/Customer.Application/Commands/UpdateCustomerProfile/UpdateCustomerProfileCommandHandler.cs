using Microsoft.Extensions.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Commands.Common;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Commands.UpdateCustomerProfile;

public sealed class UpdateCustomerProfileCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<UpdateCustomerProfileCommandHandler> logger)
    : CustomerCommandHandler<UpdateCustomerProfileCommand>(customers, unitOfWork, timeProvider, logger)
{
    protected override void Apply(Customer customer, UpdateCustomerProfileCommand command, DateTime utcNow) =>
        customer.UpdateProfile(PersonName.Create(command.FirstName, command.LastName), PhoneNumber.Create(command.PhoneNumber), utcNow);
}
