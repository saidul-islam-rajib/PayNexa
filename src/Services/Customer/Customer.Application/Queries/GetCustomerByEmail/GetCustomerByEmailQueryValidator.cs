using FluentValidation;
using PayNexa.Customers.Application.Validators;

namespace PayNexa.Customers.Application.Queries.GetCustomerByEmail;

public sealed class GetCustomerByEmailQueryValidator : AbstractValidator<GetCustomerByEmailQuery>
{
    public GetCustomerByEmailQueryValidator() => RuleFor(query => query.Email).ValidEmail();
}
