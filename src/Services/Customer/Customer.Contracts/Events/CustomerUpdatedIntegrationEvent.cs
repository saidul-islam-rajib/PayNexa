namespace PayNexa.Customers.Contracts.Events;

public sealed record CustomerUpdatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    long Version)
{
    public const string EventType = "customer.updated";

    public int EventVersion { get; init; } = 1;
}
