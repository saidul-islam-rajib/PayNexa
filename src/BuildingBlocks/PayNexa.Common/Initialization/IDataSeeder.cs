namespace PayNexa.Common.Initialization;

public interface IDataSeeder
{
    int Order { get; }

    string Name { get; }

    Task SeedAsync(CancellationToken cancellationToken);
}
