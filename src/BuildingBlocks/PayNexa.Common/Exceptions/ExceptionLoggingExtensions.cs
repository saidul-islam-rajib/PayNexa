namespace PayNexa.Common.Exceptions;

public static class ExceptionLoggingExtensions
{
    private const string LoggedKey = "PayNexa.Logged";

    public static void MarkAsLogged(this Exception exception) => exception.Data[LoggedKey] = true;

    public static bool IsLogged(this Exception exception) => exception.Data[LoggedKey] is true;
}
