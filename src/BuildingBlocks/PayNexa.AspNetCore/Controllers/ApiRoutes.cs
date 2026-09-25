namespace PayNexa.AspNetCore.Controllers;

public static class ApiRoutes
{
    public const string Resource = "api/v{version:apiVersion}/[controller]";
    public const string Action = "[action]";
    public const string ById = "{id:guid}";
    public const string ByIdAction = $"{ById}/{Action}";

    public static class Parameters
    {
        public const string Id = "id";
        public const string Version = "version";
    }
}
