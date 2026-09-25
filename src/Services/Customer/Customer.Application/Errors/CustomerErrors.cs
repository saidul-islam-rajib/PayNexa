using PayNexa.Common.Results;

namespace PayNexa.Customers.Application.Errors;

public static class CustomerErrors
{
    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("Customer.EmailAlreadyRegistered", "A customer with this email address already exists.");

    public static readonly Error ConcurrentModification =
        Error.Conflict("Customer.ConcurrentModification", "The customer was modified by another request. Reload it and try again.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Customer.NotFound", $"Customer '{id}' was not found.");
}
