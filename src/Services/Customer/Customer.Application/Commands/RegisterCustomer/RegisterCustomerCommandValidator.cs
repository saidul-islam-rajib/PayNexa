using FluentValidation;
using Microsoft.Extensions.Options;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator(TimeProvider timeProvider, IOptions<CustomerOptions> options)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.FirstName).ValidPersonName();
        RuleFor(command => command.LastName).ValidPersonName();
        RuleFor(command => command.Email).ValidEmail();
        RuleFor(command => command.PhoneNumber).ValidPhoneNumber();
        RuleFor(command => command.DateOfBirth).ValidDateOfBirth(timeProvider, options.Value.ToPolicy());
        RuleFor(command => command.Address).ValidAddress().When(command => command.Address is not null);
    }
}
