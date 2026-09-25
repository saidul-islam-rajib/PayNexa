namespace PayNexa.SharedKernel.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    protected Entity()
    {
        Id = default!;
    }

    public TId Id { get; protected set; }

    public bool Equals(Entity<TId>? other) =>
        other is not null && other.GetType() == GetType() && Id.Equals(other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
