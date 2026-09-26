namespace PayNexa.Common.Persistence;

public interface IOutbox
{
    void Enqueue<TMessage>(TMessage message)
        where TMessage : class;
}
