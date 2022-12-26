namespace Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;

public interface IMongoIndexesApplier
{
    Task ApplyAllIndexes(CancellationToken cancellationToken);
}