using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.UpdateCustomerProfile;

public sealed class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
    public UpdateCustomerProfileCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.FirstName).ValidPersonName();
        RuleFor(command => command.LastName).ValidPersonName();
        RuleFor(command => command.PhoneNumber).ValidPhoneNumber();
    }
}
