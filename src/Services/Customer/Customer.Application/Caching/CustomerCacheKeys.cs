namespace PayNexa.Customers.Application.Caching;

public static class CustomerCacheKeys
{
    public static string Profile(Guid customerId) => $"customer:{customerId:N}:profile";
}
