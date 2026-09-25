namespace PayNexa.Customers.Contracts.Events;

public sealed record CustomerCreatedIntegrationEvent(
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
    public const string EventType = "customer.created";

    public int EventVersion { get; init; } = 1;
}
