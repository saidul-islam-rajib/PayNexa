namespace PayNexa.SharedKernel.Domain;

public interface ISingleValueObject
{
    object RawValue { get; }
}

public abstract class SingleValueObject<TValue> : ValueObject, ISingleValueObject
    where TValue : notnull
{
    protected SingleValueObject(TValue value) => Value = value;

    public TValue Value { get; }

    object ISingleValueObject.RawValue => Value;

    public override string ToString() => Value.ToString() ?? string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
