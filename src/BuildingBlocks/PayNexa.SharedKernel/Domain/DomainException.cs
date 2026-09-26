using PayNexa.SharedKernel.Results;

namespace PayNexa.SharedKernel.Domain;

public sealed class DomainException(Error error) : Exception(error.Description)
{
    public Error Error { get; } = error;
}
