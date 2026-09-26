using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PayNexa.Common.Security;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.SqlServer.Auditing;

internal sealed class AuditStamper(ICurrentActor currentActor, TimeProvider timeProvider)
{
    public void Stamp(DbContext dbContext)
    {
        dbContext.ChangeTracker.DetectChanges();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var actor = currentActor.Id;

        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditable>().ToArray())
        {
            if (entry.State == EntityState.Added)
            {
                StampCreated(entry, now, actor);
            }
            else if (entry.State != EntityState.Deleted && HasChanges(entry))
            {
                StampUpdated(entry, now, actor);
            }
        }
    }

    private static void StampCreated(EntityEntry<IAuditable> entry, DateTime now, string actor)
    {
        entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = now;
        entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = actor;
        entry.Property(nameof(IAuditable.UpdatedAtUtc)).CurrentValue = now;
        entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = actor;

        if (entry.Entity is IVersioned)
        {
            entry.Property(nameof(IVersioned.Version)).CurrentValue = 1L;
        }
    }

    private static void StampUpdated(EntityEntry<IAuditable> entry, DateTime now, string actor)
    {
        entry.Property(nameof(IAuditable.UpdatedAtUtc)).CurrentValue = now;
        entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = actor;

        if (entry.Entity is IVersioned)
        {
            var version = entry.Property(nameof(IVersioned.Version));
            version.CurrentValue = (long)version.OriginalValue! + 1;
        }
    }

    private static bool HasChanges(EntityEntry<IAuditable> entry) =>
        entry.State == EntityState.Modified
        || entry.Entity is IHasDomainEvents { DomainEvents.Count: > 0 }
        || entry.References.Any(reference => reference.TargetEntry is { State: EntityState.Added or EntityState.Modified or EntityState.Deleted } target
                                             && target.Metadata.IsOwned());
}
