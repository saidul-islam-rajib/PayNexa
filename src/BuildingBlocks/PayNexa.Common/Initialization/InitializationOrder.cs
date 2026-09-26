namespace PayNexa.Common.Initialization;

public static class InitializationOrder
{
    public const int WriteStore = 100;
    public const int ReadStore = 200;
    public const int MessageBroker = 300;
}
