using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.SuspendCustomer;

public sealed class SuspendCustomerCommandValidator : AbstractValidator<SuspendCustomerCommand>
{
    public SuspendCustomerCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.Reason).ValidReason();
    }
}
