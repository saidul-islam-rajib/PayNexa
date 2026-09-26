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

    public AddressReadModel? Address { get; init; }

    public required string Status { get; init; }

    public string? StatusReason { get; init; }

    public required string KycStatus { get; init; }

    public string? KycRejectionReason { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required string CreatedBy { get; init; }

    public required DateTime UpdatedAtUtc { get; init; }

    public required string UpdatedBy { get; init; }

    public required long Version { get; init; }
}

internal sealed class AddressReadModel
{
    public required string Line1 { get; init; }

    public string? Line2 { get; init; }

    public required string City { get; init; }

    public string? State { get; init; }

    public required string PostalCode { get; init; }

    public required string CountryCode { get; init; }
}
