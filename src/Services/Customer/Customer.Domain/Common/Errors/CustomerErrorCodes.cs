namespace PayNexa.Customers.Domain.Common.Errors;

public static class CustomerErrorCodes
{
    public const string NotFound = "Customer.NotFound";
    public const string EmailAlreadyRegistered = "Customer.EmailAlreadyRegistered";
    public const string ConcurrentModification = "Customer.ConcurrentModification";
    public const string Closed = "Customer.Closed";
    public const string NotActive = "Customer.NotActive";
    public const string NotSuspended = "Customer.NotSuspended";
    public const string InvalidName = "Customer.InvalidName";
    public const string InvalidEmail = "Customer.InvalidEmail";
    public const string InvalidPhoneNumber = "Customer.InvalidPhoneNumber";
    public const string InvalidDateOfBirth = "Customer.InvalidDateOfBirth";
    public const string BelowMinimumAge = "Customer.BelowMinimumAge";
    public const string InvalidAddress = "Customer.InvalidAddress";
    public const string InvalidReason = "Customer.InvalidReason";
    public const string KycAlreadyVerified = "Customer.KycAlreadyVerified";
    public const string KycNotPending = "Customer.KycNotPending";
    public const string KycNotVerified = "Customer.KycNotVerified";
    public const string AddressRequiredForKyc = "Customer.AddressRequiredForKyc";
}
