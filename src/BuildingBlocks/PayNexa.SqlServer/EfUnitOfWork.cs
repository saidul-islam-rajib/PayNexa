using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayNexa.Common.DomainEvents;
using PayNexa.Common.Persistence;
using PayNexa.SharedKernel.Domain;
using PayNexa.SqlServer.Auditing;

namespace PayNexa.SqlServer;

internal sealed class EfUnitOfWork<TDbContext>(
    TDbContext dbContext,
    AuditStamper auditStamper,
    IDomainEventDispatcher domainEventDispatcher)
    : IUnitOfWork
    where TDbContext : DbContext
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;
    private const int MaxDomainEventRounds = 10;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        auditStamper.Stamp(dbContext);
        await DispatchDomainEventsAsync(cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(SqlServerErrorMessages.ConcurrencyConflict, exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation })
        {
            throw new UniqueConstraintViolationException(SqlServerErrorMessages.UniqueConstraintViolation, exception);
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        for (var round = 0; round < MaxDomainEventRounds; round++)
        {
            var domainEvents = dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .SelectMany(entry => entry.Entity.DequeueDomainEvents())
                .ToArray();

            if (domainEvents.Length == 0)
            {
                return;
            }

            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        throw new InvalidOperationException(SqlServerErrorMessages.DomainEventCascadeTooDeep);
    }
}
