namespace PayNexa.Common.Security;

public sealed class SystemActor : ICurrentActor
{
    public const string SystemId = "system";

    public string Id => SystemId;
}
