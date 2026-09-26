using FluentValidation;
using PayNexa.Common.Validation;
using PayNexa.Customers.Contracts.Common;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Validators;

public static class CustomerRules
{
    public static IRuleBuilderOptions<T, Guid> ValidCustomerId<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEmpty();

    public static IRuleBuilderOptions<T, string> ValidPersonName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(PersonName.IsValidPart)
            .WithMessage(ValidationMessages.MaxLength(PersonName.MaxLength));

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(Email.IsValid)
            .WithMessage(ValidationMessages.InvalidEmail);

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(PhoneNumber.IsValid)
            .WithMessage(ValidationMessages.InvalidPhoneNumber);

    public static IRuleBuilderOptions<T, string> ValidReason<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(StatusReason.IsValid)
            .WithMessage(ValidationMessages.MaxLength(StatusReason.MaxLength));

    public static IRuleBuilderOptions<T, DateOnly> ValidDateOfBirth<T>(this IRuleBuilder<T, DateOnly> rule, TimeProvider timeProvider, CustomerPolicy policy) =>
        rule.Must(dateOfBirth => Customer.IsValidDateOfBirth(dateOfBirth, timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage(ValidationMessages.DateBetween(Customer.EarliestDateOfBirth))
            .Must(dateOfBirth => policy.MeetsMinimumAge(dateOfBirth, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)))
            .WithMessage(ValidationMessages.MinimumAge(policy.MinimumAgeYears));

    public static IRuleBuilderOptions<T, AddressDto?> ValidAddress<T>(this IRuleBuilder<T, AddressDto?> rule) =>
        rule.SetValidator(new AddressValidator()!);

    private sealed class AddressValidator : AbstractValidator<AddressDto>
    {
        public AddressValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(address => address.Line1).NotEmpty().MaximumLength(Address.LineMaxLength);
            RuleFor(address => address.Line2).MaximumLength(Address.LineMaxLength);
            RuleFor(address => address.City).NotEmpty().MaximumLength(Address.CityMaxLength);
            RuleFor(address => address.State).MaximumLength(Address.StateMaxLength);
            RuleFor(address => address.PostalCode).NotEmpty().MaximumLength(Address.PostalCodeMaxLength);
            RuleFor(address => address.CountryCode)
                .Must(Address.IsValidCountryCode)
                .WithMessage(ValidationMessages.CountryCode);
        }
    }
}
