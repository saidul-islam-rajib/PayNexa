using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.ReactivateCustomer;

public sealed class ReactivateCustomerCommandValidator : AbstractValidator<ReactivateCustomerCommand>
{
    public ReactivateCustomerCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
    }
}
