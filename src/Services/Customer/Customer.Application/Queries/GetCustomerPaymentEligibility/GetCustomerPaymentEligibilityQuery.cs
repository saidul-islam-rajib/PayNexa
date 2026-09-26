using Mediator;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerPaymentEligibility;

public sealed record GetCustomerPaymentEligibilityQuery(Guid CustomerId) : IQuery<Result<PaymentEligibilityResponse>>;
