using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Commands.ChangeCustomerAddress;

public sealed class ChangeCustomerAddressCommandValidator : AbstractValidator<ChangeCustomerAddressCommand>
{
    public ChangeCustomerAddressCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command.CustomerId).ValidCustomerId();
        RuleFor(command => command.Address).NotNull().ValidAddress();
    }
}
