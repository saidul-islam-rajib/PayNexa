using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators,
    ILogger<ValidationBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IValidator<TMessage>[] _validators = validators.ToArray();

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        var error = await ValidateAsync(message, cancellationToken);

        return error is null
            ? await next(message, cancellationToken)
            : ResultFailureFactory<TResponse>.Create(error);
    }

    private async Task<Error?> ValidateAsync(TMessage message, CancellationToken cancellationToken)
    {
        using var step = logger.BeginStep("Validation");

        var context = new ValidationContext<TMessage>(message);
        var results = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(result => result.Errors).ToArray();

        if (failures.Length == 0)
        {
            step.Succeeded();
            return null;
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(), StringComparer.Ordinal);

        step.WithProperty("InvalidFields", errors.Keys.ToArray())
            .Failed($"{failures.Length} validation error(s)");

        return Error.Validation(errors);
    }
}
