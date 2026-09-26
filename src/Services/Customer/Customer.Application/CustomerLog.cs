using Microsoft.Extensions.Logging;

namespace PayNexa.Customers.Application;

internal static partial class CustomerLog
{
    [LoggerMessage(EventId = CustomerLogEvents.Registered, Level = LogLevel.Information, Message = "Customer {CustomerId} registered")]
    public static partial void Registered(ILogger logger, Guid customerId);

    [LoggerMessage(EventId = CustomerLogEvents.Changed, Level = LogLevel.Information, Message = "{CustomerCommand} applied to customer {CustomerId}; now at version {CustomerVersion}")]
    public static partial void Changed(ILogger logger, string customerCommand, Guid customerId, long customerVersion);
}
