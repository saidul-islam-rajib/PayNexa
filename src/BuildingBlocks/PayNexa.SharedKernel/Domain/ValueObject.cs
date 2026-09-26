namespace PayNexa.SharedKernel.Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && other.GetType() == GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject valueObject && Equals(valueObject);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(GetType().GetHashCode(), (hash, component) => HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
