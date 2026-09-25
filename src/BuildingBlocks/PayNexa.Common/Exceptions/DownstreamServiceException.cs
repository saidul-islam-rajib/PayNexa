namespace PayNexa.Common.Exceptions;

public enum DownstreamFailureKind
{
    Timeout,
    CircuitOpen,
    Unreachable,
}

public sealed class DownstreamServiceException(
    string targetService,
    string operation,
    DownstreamFailureKind failureKind,
    Exception? innerException = null)
    : Exception($"Call to {targetService} ({operation}) failed: {failureKind}.", innerException)
{
    public string TargetService { get; } = targetService;

    public string Operation { get; } = operation;

    public DownstreamFailureKind FailureKind { get; } = failureKind;
}
