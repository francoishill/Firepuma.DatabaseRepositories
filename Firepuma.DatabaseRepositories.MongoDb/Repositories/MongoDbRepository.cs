using System.Linq.Expressions;
using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.DatabaseRepositories.Abstractions.Exceptions;
using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;
using Firepuma.DatabaseRepositories.MongoDb.Queries;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Firepuma.DatabaseRepositories.MongoDb.Repositories;

public abstract class MongoDbRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly ILogger Logger;
    protected readonly IMongoCollection<T> Collection;

    protected MongoDbRepository(
        ILogger logger,
        IMongoCollection<T> collection)
    {
        Logger = logger;
        Collection = collection;
    }

    protected virtual string CollectionNameForLogs => Collection.CollectionNamespace.FullName;

    protected virtual string GenerateETag() => $"{DateTimeOffset.UtcNow:O}-{Guid.NewGuid().ToString()}";

    public async Task<IEnumerable<T>> GetItemsAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var queryable = ApplyQuery(querySpecification);

        var items = await queryable.ToListAsync(cancellationToken);

        Logger.LogDebug(
            "Fetched {Count} items in collection {Collection}",
            items.Count, CollectionNameForLogs);

        return items;
    }

    public async Task<int> GetItemsCountAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var queryable = ApplyQuery(querySpecification);

        var count = await queryable.CountAsync(cancellationToken);

        Logger.LogDebug(
            "Counted {Count} items in collection {Collection}",
            count, CollectionNameForLogs);

        return count;
    }

    public async Task<T?> GetItemOrDefaultAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await Collection
            .Find(i => i.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        Logger.LogDebug(
            "Fetched item id {Id} from collection {Collection}",
            id, CollectionNameForLogs);

        return item;
    }

    public async Task<T?> GetItemOrDefaultAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var queryable = ApplyQuery(querySpecification);
        try
        {
            return await queryable.SingleOrDefaultAsync(cancellationToken);
        }
        catch (InvalidOperationException invalidOperationException) when (invalidOperationException.Message.Contains("Sequence contains more than one element", StringComparison.OrdinalIgnoreCase))
        {
            var queryType = querySpecification.GetType();
            var queryTypeName = queryType.FullName ?? queryType.AssemblyQualifiedName;
            throw new MultipleResultsInsteadOfSingleException($"Query type: {queryTypeName}");
        }
    }

    public async Task<T> GetItemAsync(string id, CancellationToken cancellationToken = default)
    {
        var itemOrDefault = await GetItemOrDefaultAsync(id, cancellationToken);

        if (itemOrDefault == null)
        {
            throw new DatabaseItemNotFoundException(typeof(T), id);
        }

        return itemOrDefault;
    }

    public async Task<T> GetItemAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var itemOrDefault = await GetItemOrDefaultAsync(querySpecification, cancellationToken);

        if (itemOrDefault == null)
        {
            throw new DatabaseItemNotFoundException(typeof(T), $"query type {querySpecification.GetType().Name}");
        }

        return itemOrDefault;
    }

    public async Task<T> AddItemAsync(T item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id is required to be non-empty before calling MongoDbRepository.AddItemAsync");
        }

        item.ETag = GenerateETag();

        await Collection.InsertOneAsync(item, cancellationToken: cancellationToken);

        Logger.LogInformation(
            "Added item id {Id} to collection {Collection}",
            item.Id, CollectionNameForLogs);

        return item;
    }

    public async Task<T> UpsertItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id is required to be non-empty before calling MongoDbRepository.UpsertItemAsync");
        }

        var oldETag = item.ETag;

        var options = new ReplaceOptions
        {
            IsUpsert = true,
        };

        Expression<Func<T, bool>> filter =
            ignoreETag
                ? i => i.Id == item.Id
                : i => i.Id == item.Id && i.ETag == oldETag;

        item.ETag = GenerateETag();

        var replaceResult = await Collection.ReplaceOneAsync(filter, item, options, cancellationToken);

        if (ignoreETag && replaceResult.MatchedCount == 0)
        {
            throw new DatabaseItemNotFoundException(typeof(T), item.Id);
        }

        if (!ignoreETag)
        {
            VerifyReplacedExactlyOneDocument(options, replaceResult);
        }

        Logger.LogInformation(
            "Upserted item id {Id} in collection {Collection}",
            item.Id, CollectionNameForLogs);

        return item;
    }

    public async Task<T> UpsertItemAsync(
        T item,
        CancellationToken cancellationToken = default)
    {
        return await UpsertItemAsync(item, ignoreETag: false, cancellationToken);
    }

    protected async Task UpdateItemAsync(
        T item,
        UpdateDefinition<T> updateDefinition,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id is required to be non-empty before calling MongoDbRepository.UpdateItemAsync");
        }

        var oldETag = item.ETag;

        var options = new UpdateOptions
        {
            IsUpsert = false,
        };

        Expression<Func<T, bool>> filter =
            ignoreETag
                ? i => i.Id == item.Id
                : i => i.Id == item.Id && i.ETag == oldETag;

        var newETag = GenerateETag();

        var finalUpdateDefinition =
            Builders<T>.Update.Combine(
                updateDefinition,
                Builders<T>.Update.Set(i => i.ETag, newETag));

        // ETag field won't be updated automatically by the SDK since we used an Update Definition
        item.ETag = newETag;

        var updateResult = await Collection.UpdateOneAsync(filter, finalUpdateDefinition, options, cancellationToken);

        if (ignoreETag && updateResult.MatchedCount == 0)
        {
            throw new DatabaseItemNotFoundException(typeof(T), item.Id);
        }

        if (!ignoreETag)
        {
            VerifyUpdatedExactlyOneDocument(options, updateResult);
        }

        Logger.LogInformation(
            "Updated item id {Id} in collection {Collection}",
            item.Id, CollectionNameForLogs);
    }

    // ReSharper disable once UnusedMember.Global
    protected async Task UpdateItemAsync(
        T item,
        UpdateDefinition<T> updateDefinition,
        CancellationToken cancellationToken = default)
    {
        await UpdateItemAsync(item, updateDefinition, ignoreETag: false, cancellationToken);
    }

    public async Task DeleteItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default)
    {
        var oldETag = item.ETag;

        Expression<Func<T, bool>> filter =
            ignoreETag
                ? i => i.Id == item.Id
                : i => i.Id == item.Id && i.ETag == oldETag;

        var deleteResult = await Collection.DeleteOneAsync(filter, cancellationToken);

        if (!ignoreETag)
        {
            VerifyDeleteExactlyOneDocument(deleteResult);
        }

        Logger.LogInformation(
            "Deleted item id {Id} from collection {Collection}",
            item.Id, CollectionNameForLogs);
    }

    public async Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken = default)
    {
        await DeleteItemAsync(item, ignoreETag: false, cancellationToken);
    }

    private static void VerifyReplacedExactlyOneDocument(
        ReplaceOptions options,
        ReplaceOneResult replaceResult)
    {
        if (!replaceResult.IsAcknowledged)
        {
            throw new InvalidOperationException("Expected mongo ReplaceOneAsync to be automatically acknowledged");
        }

        if (!replaceResult.IsModifiedCountAvailable)
        {
            throw new InvalidOperationException("Expected mongo ReplaceOneAsync to have IsModifiedCountAvailable == TRUE");
        }

        if (replaceResult.ModifiedCount > 1 || replaceResult.MatchedCount > 1)
        {
            throw new InvalidOperationException("Expected mongo ReplaceOneAsync to have ModifiedCount == 1 and MatchedCount == 1");
        }

        if (replaceResult.ModifiedCount == 0 || replaceResult.MatchedCount == 0)
        {
            if (replaceResult.UpsertedId != null && options is { IsUpsert: true })
            {
                // do nothing
            }
            else
            {
                throw new DocumentETagMismatchException();
            }
        }
    }

    private static void VerifyUpdatedExactlyOneDocument(
        UpdateOptions options,
        UpdateResult updateResult)
    {
        if (!updateResult.IsAcknowledged)
        {
            throw new InvalidOperationException("Expected mongo UpdateOneAsync to be automatically acknowledged");
        }

        if (!updateResult.IsModifiedCountAvailable)
        {
            throw new InvalidOperationException("Expected mongo UpdateOneAsync to have IsModifiedCountAvailable == TRUE");
        }

        if (updateResult.ModifiedCount > 1 || updateResult.MatchedCount > 1)
        {
            throw new InvalidOperationException("Expected mongo UpdateOneAsync to have ModifiedCount == 1 and MatchedCount == 1");
        }

        if (updateResult.ModifiedCount == 0 || updateResult.MatchedCount == 0)
        {
            if (updateResult.UpsertedId != null && options is { IsUpsert: true })
            {
                // do nothing
            }
            else
            {
                throw new DocumentETagMismatchException();
            }
        }
    }

    private static void VerifyDeleteExactlyOneDocument(
        DeleteResult deleteResult)
    {
        if (!deleteResult.IsAcknowledged)
        {
            throw new InvalidOperationException("Expected mongo ReplaceOneAsync to be automatically acknowledged");
        }

        if (deleteResult.DeletedCount > 1)
        {
            throw new InvalidOperationException("Expected mongo ReplaceOneAsync to have ModifiedCount == 1 and MatchedCount == 1");
        }

        if (deleteResult.DeletedCount == 0)
        {
            throw new DocumentETagMismatchException();
        }
    }

    protected virtual IMongoQueryable<T> ApplyQuery(IQuerySpecification<T> querySpecification)
    {
        var evaluator = new MongoDbQuerySpecificationEvaluator<T>();
        var inputQuery = Collection.AsQueryable();
        return (evaluator.GetQuery(inputQuery, querySpecification) as IMongoQueryable<T>)!;
    }
}