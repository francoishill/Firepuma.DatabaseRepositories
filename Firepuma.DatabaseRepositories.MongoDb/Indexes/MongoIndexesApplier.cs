using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;
using Firepuma.DatabaseRepositories.MongoDb.Indexes.Collections;
using Microsoft.Extensions.Logging;

namespace Firepuma.DatabaseRepositories.MongoDb.Indexes;

internal class MongoIndexesApplier : IMongoIndexesApplier
{
    private readonly ILogger<MongoIndexesApplier> _logger;
    private readonly IEnumerable<IMongoCollectionIndexApplier> _collectionIndexAppliers;

    public MongoIndexesApplier(
        ILogger<MongoIndexesApplier> logger,
        IEnumerable<IMongoCollectionIndexApplier> collectionIndexAppliers)
    {
        _logger = logger;
        _collectionIndexAppliers = collectionIndexAppliers;
    }

    public async Task ApplyAllIndexes(CancellationToken cancellationToken)
    {
        var tasks = _collectionIndexAppliers.Select(indexApplier => indexApplier.ApplyIndexes(cancellationToken));
        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Applied indexes using {Count} collection index appliers",
            _collectionIndexAppliers.Count());
    }
}