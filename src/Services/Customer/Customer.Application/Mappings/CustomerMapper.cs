using PayNexa.Customers.Contracts.Common;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;
using Riok.Mapperly.Abstractions;

namespace PayNexa.Customers.Application.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CustomerMapper
{
    [MapProperty([nameof(Customer.Name), nameof(PersonName.FirstName)], nameof(CustomerResponse.FirstName))]
    [MapProperty([nameof(Customer.Name), nameof(PersonName.LastName)], nameof(CustomerResponse.LastName))]
    public static partial CustomerResponse ToResponse(this Customer customer);

    [MapProperty([nameof(Customer.Name), nameof(PersonName.FirstName)], nameof(CustomerSnapshot.FirstName))]
    [MapProperty([nameof(Customer.Name), nameof(PersonName.LastName)], nameof(CustomerSnapshot.LastName))]
    public static partial CustomerSnapshot ToSnapshot(this Customer customer);

    public static partial AddressDto ToDto(this Address address);

    public static PaymentEligibilityResponse ToResponse(this PaymentEligibility eligibility, Guid customerId) =>
        new(customerId, eligibility.IsEligible, eligibility.Reasons.Select(ToReason).ToList());

    private static EligibilityReasonResponse ToReason(Error error) => new(error.Code, error.Description);

    private static Guid MapCustomerId(CustomerId id) => id.Value;

    private static string MapEmail(Email email) => email.Value;

    private static string MapPhoneNumber(PhoneNumber phoneNumber) => phoneNumber.Value;

    private static string? MapStatusReason(StatusReason? reason) => reason?.Value;
}
