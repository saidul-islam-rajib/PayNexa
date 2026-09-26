using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Domain.CustomerAggregate;

public sealed record PaymentEligibility(bool IsEligible, IReadOnlyList<Error> Reasons)
{
    public static PaymentEligibility From(IReadOnlyList<Error> reasons) => new(reasons.Count == 0, reasons);
}
