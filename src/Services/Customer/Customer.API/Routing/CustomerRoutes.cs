using PayNexa.AspNetCore.Controllers;

namespace PayNexa.Customers.API.Routing;

public static class CustomerRoutes
{
    public const string Email = $"{ApiRoutes.ById}/email";
    public const string Address = $"{ApiRoutes.ById}/address";
    public const string KycAction = $"{ApiRoutes.ById}/kyc/{ApiRoutes.Action}";

    public static class KycActions
    {
        public const string Verify = "Verify";
        public const string Reject = "Reject";
    }
}
