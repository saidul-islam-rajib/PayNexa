namespace PayNexa.Common.Persistence;

public interface IOutboxMessageHandler<in TMessage>
    where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
