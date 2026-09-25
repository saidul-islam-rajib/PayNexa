namespace PayNexa.Customers.Domain.DomainExceptions;

public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
