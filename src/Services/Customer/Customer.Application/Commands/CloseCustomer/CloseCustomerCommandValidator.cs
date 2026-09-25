using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.CloseCustomer;

public sealed class CloseCustomerCommandValidator : AbstractValidator<CloseCustomerCommand>
{
    public CloseCustomerCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.Reason).ValidReason();
    }
}
