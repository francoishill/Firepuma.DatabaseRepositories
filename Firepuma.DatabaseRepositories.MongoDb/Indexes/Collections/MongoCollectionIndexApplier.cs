﻿using Firepuma.DatabaseRepositories.Abstractions.Entities;
using MongoDB.Driver;

namespace Firepuma.DatabaseRepositories.MongoDb.Indexes.Collections;

internal class MongoCollectionIndexApplier<TEntity> : IMongoCollectionIndexApplier
    where TEntity : IEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly Func<IEnumerable<CreateIndexModel<TEntity>>> _indexesFactory;

    public MongoCollectionIndexApplier(
        IMongoCollection<TEntity> collection,
        Func<IEnumerable<CreateIndexModel<TEntity>>> indexesFactory)
    {
        _collection = collection;
        _indexesFactory = indexesFactory;
    }

    public async Task ApplyIndexes(CancellationToken cancellationToken)
    {
        var indexes = _indexesFactory().ToArray();
        if (!indexes.Any())
        {
            return;
        }

        await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}