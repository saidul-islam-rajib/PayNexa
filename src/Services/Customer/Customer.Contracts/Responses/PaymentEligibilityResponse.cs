namespace PayNexa.Customers.Contracts.Responses;

public sealed record PaymentEligibilityResponse(
    Guid CustomerId,
    bool IsEligible,
    IReadOnlyList<EligibilityReasonResponse> Reasons);

public sealed record EligibilityReasonResponse(string Code, string Description);
