using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.RejectCustomerKyc;

public sealed class RejectCustomerKycCommandValidator : AbstractValidator<RejectCustomerKycCommand>
{
    public RejectCustomerKycCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.Reason).ValidReason();
    }
}
