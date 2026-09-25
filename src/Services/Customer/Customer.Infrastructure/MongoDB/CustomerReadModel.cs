using PayNexa.Customers.Contracts.Responses;
using PayNexa.MongoDb.ReadModels;

namespace PayNexa.Customers.Infrastructure.MongoDB;

internal sealed class CustomerReadModel : IVersionedReadModel
{
    public const string CollectionName = "customers";

    public required Guid Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required string PhoneNumber { get; init; }

    public required DateOnly DateOfBirth { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required DateTime UpdatedAtUtc { get; init; }

    public required long Version { get; init; }

    public CustomerResponse ToResponse() => new(
        Id,
        FirstName,
        LastName,
        Email,
        PhoneNumber,
        DateOfBirth,
        Status,
        CreatedAtUtc,
        UpdatedAtUtc);
}
