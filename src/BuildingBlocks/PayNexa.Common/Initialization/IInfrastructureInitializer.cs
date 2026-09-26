namespace PayNexa.Common.Initialization;

public interface IInfrastructureInitializer
{
    int Order { get; }

    string Name { get; }

    Task InitializeAsync(CancellationToken cancellationToken);
}
