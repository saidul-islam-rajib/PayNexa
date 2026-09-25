using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayNexa.Common.Correlation;
using PayNexa.Common.Persistence;

namespace PayNexa.SqlServer.Outbox;

internal sealed class EfOutbox<TDbContext>(TDbContext dbContext, TimeProvider timeProvider) : IOutbox
    where TDbContext : DbContext
{
    public void Enqueue<TMessage>(TMessage message)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = OutboxSerializer.TypeNameOf(typeof(TMessage)),
            Payload = JsonSerializer.Serialize(message, OutboxSerializer.Options),
            OccurredAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            CorrelationId = CorrelationContext.Current,
            TraceParent = Activity.Current?.Id,
        });
    }
}
