using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed class Address : ValueObject
{
    public const int LineMaxLength = 200;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PostalCodeMaxLength = 20;
    public const int CountryCodeLength = 2;

    private Address(string line1, string? line2, string city, string? state, string postalCode, string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        CountryCode = countryCode;
    }

    public string Line1 { get; private set; }

    public string? Line2 { get; private set; }

    public string City { get; private set; }

    public string? State { get; private set; }

    public string PostalCode { get; private set; }

    public string CountryCode { get; private set; }

    public static bool IsValidCountryCode(string? countryCode) =>
        countryCode is { Length: CountryCodeLength } && countryCode.All(char.IsAsciiLetter);

    public static Address Create(string line1, string? line2, string city, string? state, string postalCode, string countryCode)
    {
        if (!IsRequired(line1, LineMaxLength)
            || !IsOptional(line2, LineMaxLength)
            || !IsRequired(city, CityMaxLength)
            || !IsOptional(state, StateMaxLength)
            || !IsRequired(postalCode, PostalCodeMaxLength)
            || countryCode?.Trim() is not { } country
            || !IsValidCountryCode(country))
        {
            throw new DomainException(Errors.Customer.InvalidAddress);
        }

        return new Address(
            line1.Trim(),
            string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
            city.Trim(),
            string.IsNullOrWhiteSpace(state) ? null : state.Trim(),
            postalCode.Trim(),
            country.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return CountryCode;
    }

    private static bool IsRequired(string? value, int maxLength) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= maxLength;

    private static bool IsOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength;
}
