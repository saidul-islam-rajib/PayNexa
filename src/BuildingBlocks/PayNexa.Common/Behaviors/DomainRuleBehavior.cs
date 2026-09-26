using Mediator;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Common.Behaviors;

public sealed class DomainRuleBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (DomainException exception)
        {
            return ResultFailureFactory<TResponse>.Create(exception.Error);
        }
    }
}
