using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.VerifyCustomerKyc;

public sealed class VerifyCustomerKycCommandValidator : AbstractValidator<VerifyCustomerKycCommand>
{
    public VerifyCustomerKycCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
    }
}
