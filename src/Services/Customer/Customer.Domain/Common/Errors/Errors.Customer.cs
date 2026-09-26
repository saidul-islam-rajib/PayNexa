using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Domain.Common.Errors;

public static partial class Errors
{
    public static class Customer
    {
        public static Error EmailAlreadyRegistered => Error.Conflict(CustomerErrorCodes.EmailAlreadyRegistered, CustomerErrorMessages.EmailAlreadyRegistered);

        public static Error ConcurrentModification => Error.Conflict(CustomerErrorCodes.ConcurrentModification, CustomerErrorMessages.ConcurrentModification);

        public static Error Closed => Error.Conflict(CustomerErrorCodes.Closed, CustomerErrorMessages.Closed);

        public static Error NotActive => Error.Conflict(CustomerErrorCodes.NotActive, CustomerErrorMessages.NotActive);

        public static Error NotSuspended => Error.Conflict(CustomerErrorCodes.NotSuspended, CustomerErrorMessages.NotSuspended);

        public static Error KycAlreadyVerified => Error.Conflict(CustomerErrorCodes.KycAlreadyVerified, CustomerErrorMessages.KycAlreadyVerified);

        public static Error KycNotPending => Error.Conflict(CustomerErrorCodes.KycNotPending, CustomerErrorMessages.KycNotPending);

        public static Error KycNotVerified => Error.BusinessRule(CustomerErrorCodes.KycNotVerified, CustomerErrorMessages.KycNotVerified);

        public static Error AddressRequiredForKyc => Error.BusinessRule(CustomerErrorCodes.AddressRequiredForKyc, CustomerErrorMessages.AddressRequiredForKyc);

        public static Error InvalidName => Error.BusinessRule(
            CustomerErrorCodes.InvalidName,
            string.Format(CustomerErrorMessages.InvalidNameFormat, PersonName.MaxLength));

        public static Error InvalidEmail => Error.BusinessRule(CustomerErrorCodes.InvalidEmail, CustomerErrorMessages.InvalidEmail);

        public static Error InvalidPhoneNumber => Error.BusinessRule(CustomerErrorCodes.InvalidPhoneNumber, CustomerErrorMessages.InvalidPhoneNumber);

        public static Error InvalidAddress => Error.BusinessRule(CustomerErrorCodes.InvalidAddress, CustomerErrorMessages.InvalidAddress);

        public static Error InvalidReason => Error.BusinessRule(
            CustomerErrorCodes.InvalidReason,
            string.Format(CustomerErrorMessages.InvalidReasonFormat, StatusReason.MaxLength));

        public static Error InvalidDateOfBirth => Error.BusinessRule(
            CustomerErrorCodes.InvalidDateOfBirth,
            string.Format(CustomerErrorMessages.InvalidDateOfBirthFormat, CustomerAggregate.Customer.EarliestDateOfBirth.ToString("yyyy-MM-dd")));

        public static Error BelowMinimumAge(int minimumAgeYears) => Error.BusinessRule(
            CustomerErrorCodes.BelowMinimumAge,
            string.Format(CustomerErrorMessages.BelowMinimumAgeFormat, minimumAgeYears));

        public static Error NotFoundByEmail => Error.NotFound(CustomerErrorCodes.NotFound, CustomerErrorMessages.NotFoundByEmail);

        public static Error NotFound(Guid customerId) => Error.NotFound(
            CustomerErrorCodes.NotFound,
            string.Format(CustomerErrorMessages.NotFoundFormat, customerId));
    }
}
