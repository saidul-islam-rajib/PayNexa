namespace PayNexa.Customers.Application.Caching;

public static class CustomerCacheKeys
{
    public static readonly TimeSpan ProfileTimeToLive = TimeSpan.FromMinutes(10);

    public static string Profile(Guid customerId) => $"customer:{customerId:N}:profile";
}
