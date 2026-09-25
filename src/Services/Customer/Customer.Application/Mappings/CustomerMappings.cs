using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Entities;

namespace PayNexa.Customers.Application.Mappings;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer customer) => new(
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.Email.Value,
        customer.PhoneNumber.Value,
        customer.DateOfBirth,
        customer.Status.ToString(),
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc);

    public static CustomerCreatedIntegrationEvent ToCreatedEvent(this Customer customer, DateTime occurredAtUtc) => new(
        Guid.CreateVersion7(),
        occurredAtUtc,
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.Email.Value,
        customer.PhoneNumber.Value,
        customer.DateOfBirth,
        customer.Status.ToString(),
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc,
        customer.Version);

    public static CustomerUpdatedIntegrationEvent ToUpdatedEvent(this Customer customer, DateTime occurredAtUtc) => new(
        Guid.CreateVersion7(),
        occurredAtUtc,
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.Email.Value,
        customer.PhoneNumber.Value,
        customer.DateOfBirth,
        customer.Status.ToString(),
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc,
        customer.Version);
}
