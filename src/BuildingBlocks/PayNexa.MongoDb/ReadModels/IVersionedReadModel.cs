namespace PayNexa.MongoDb.ReadModels;

public interface IVersionedReadModel
{
    Guid Id { get; }

    long Version { get; }
}
