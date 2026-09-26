using Mediator;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerPaymentEligibility;

public sealed class GetCustomerPaymentEligibilityQueryHandler(ICustomerRepository customers)
    : IQueryHandler<GetCustomerPaymentEligibilityQuery, Result<PaymentEligibilityResponse>>
{
    public async ValueTask<Result<PaymentEligibilityResponse>> Handle(GetCustomerPaymentEligibilityQuery query, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(CustomerId.Create(query.CustomerId), cancellationToken);

        return customer is null
            ? Errors.Customer.NotFound(query.CustomerId)
            : customer.EvaluatePaymentEligibility().ToResponse(query.CustomerId);
    }
}
