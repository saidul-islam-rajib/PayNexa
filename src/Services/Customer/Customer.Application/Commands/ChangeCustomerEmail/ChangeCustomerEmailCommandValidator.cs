using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerEmail;

public sealed class ChangeCustomerEmailCommandValidator : AbstractValidator<ChangeCustomerEmailCommand>
{
    public ChangeCustomerEmailCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.Email).ValidEmail();
    }
}
