using FluentValidation;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Application.Validators;

public static class CustomerRules
{
    public static IRuleBuilderOptions<T, string> ValidPersonName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(name => name.Trim().Length <= Customer.NameMaxLength)
            .WithMessage($"'{{PropertyName}}' must be at most {Customer.NameMaxLength} characters.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(Email.IsValid)
            .WithMessage("'{PropertyName}' must be a valid email address.");

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(PhoneNumber.IsValid)
            .WithMessage("'{PropertyName}' must be in E.164 format, e.g. +8801700000000.");
}
