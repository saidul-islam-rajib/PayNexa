namespace PayNexa.Customers.Contracts.Events;

public static class CustomerChangeTypes
{
    public const string ProfileUpdated = "ProfileUpdated";
    public const string EmailChanged = "EmailChanged";
    public const string AddressChanged = "AddressChanged";
    public const string Suspended = "Suspended";
    public const string Reactivated = "Reactivated";
    public const string Closed = "Closed";
    public const string KycVerified = "KycVerified";
    public const string KycRejected = "KycRejected";
}
