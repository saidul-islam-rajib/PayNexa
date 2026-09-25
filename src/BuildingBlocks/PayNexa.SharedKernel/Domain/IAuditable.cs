namespace PayNexa.SharedKernel.Domain;

public interface IAuditable
{
    DateTime CreatedAtUtc { get; }

    string CreatedBy { get; }

    DateTime UpdatedAtUtc { get; }

    string UpdatedBy { get; }
}

public interface IVersioned
{
    long Version { get; }
}
