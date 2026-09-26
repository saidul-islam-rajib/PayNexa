namespace PayNexa.Common.Persistence;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class UniqueConstraintViolationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
