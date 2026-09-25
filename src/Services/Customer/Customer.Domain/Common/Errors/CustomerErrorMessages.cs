namespace PayNexa.Customers.Domain.Common.Errors;

public static class CustomerErrorMessages
{
    public const string NotFoundFormat = "Customer '{0}' was not found.";
    public const string NotFoundByEmail = "No customer is registered with this email address.";
    public const string EmailAlreadyRegistered = "A customer with this email address already exists.";
    public const string ConcurrentModification = "The customer was modified by another request. Reload it and try again.";
    public const string Closed = "The customer is closed; no further changes are allowed.";
    public const string NotActive = "Only an active customer can perform this operation.";
    public const string NotSuspended = "Only a suspended customer can be reactivated.";
    public const string InvalidNameFormat = "The first and last name are required and must be at most {0} characters.";
    public const string InvalidEmail = "The email address is not valid.";
    public const string InvalidPhoneNumber = "The phone number must be in E.164 format, e.g. +8801700000000.";
    public const string InvalidDateOfBirthFormat = "The date of birth must be between {0} and today.";
    public const string BelowMinimumAgeFormat = "The customer must be at least {0} years old.";
    public const string InvalidAddress = "The address requires line 1, city, postal code and a two-letter ISO country code.";
    public const string InvalidReasonFormat = "A reason is required and must be at most {0} characters.";
    public const string KycAlreadyVerified = "The customer's identity is already verified.";
    public const string KycNotPending = "Only a pending identity verification can be rejected.";
    public const string KycNotVerified = "The customer's identity has not been verified.";
    public const string AddressRequiredForKyc = "A residential address is required before identity verification.";
}
