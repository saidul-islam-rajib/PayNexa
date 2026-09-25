using FluentValidation;
using PayNexa.Customers.Application.Validators;
using PayNexa.Customers.Domain.Entities;

namespace PayNexa.Customers.Application.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator(TimeProvider timeProvider)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.FirstName).ValidPersonName();
        RuleFor(command => command.LastName).ValidPersonName();
        RuleFor(command => command.Email).ValidEmail();
        RuleFor(command => command.PhoneNumber).ValidPhoneNumber();
        RuleFor(command => command.DateOfBirth)
            .Must(dateOfBirth => Customer.IsValidDateOfBirth(dateOfBirth, timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage($"'{{PropertyName}}' must be between {Customer.EarliestDateOfBirth:yyyy-MM-dd} and today.");
    }
}
