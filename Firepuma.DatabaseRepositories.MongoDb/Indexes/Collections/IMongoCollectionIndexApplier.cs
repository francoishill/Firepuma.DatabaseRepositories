namespace Firepuma.DatabaseRepositories.MongoDb.Indexes.Collections;

internal interface IMongoCollectionIndexApplier
{
    Task ApplyIndexes(CancellationToken cancellationToken);
}