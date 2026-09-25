using System.Diagnostics.CodeAnalysis;

namespace PayNexa.SharedKernel.Results;

public class Result
{
    private readonly Error? _error;

    protected Result(Error? error) => _error = error;

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => _error is not null;

    public Error? Error => _error;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base(null) => _value = value;

    private Result(Error error) : base(error) => _value = default;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot read the value of a failed result ({Error.Code}).");

    public static Result<TValue> Success(TValue value) => new(value);

    public static new Result<TValue> Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
