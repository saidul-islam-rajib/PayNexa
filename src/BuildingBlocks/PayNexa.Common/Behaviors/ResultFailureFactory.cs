using System.Reflection;
using PayNexa.Common.Results;

namespace PayNexa.Common.Behaviors;

internal static class ResultFailureFactory<TResponse>
{
    private static readonly Func<Error, TResponse>? Factory = BuildFactory();

    public static TResponse Create(Error error) => Factory is not null
        ? Factory(error)
        : throw new InvalidOperationException(
            $"{typeof(TResponse).Name} must be {nameof(Result)} or {nameof(Result)}<T> for validated messages to return failures.");

    private static Func<Error, TResponse>? BuildFactory()
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return error => (TResponse)(object)Result.Failure(error);
        }

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return null;
        }

        var failure = responseType.GetMethod(
            nameof(Result.Failure),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            [typeof(Error)])!;

        return failure.CreateDelegate<Func<Error, TResponse>>();
    }
}
