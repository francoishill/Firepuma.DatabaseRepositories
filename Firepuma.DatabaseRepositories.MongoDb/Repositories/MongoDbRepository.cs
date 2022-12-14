using System.Linq.Expressions;
using Firepuma.DatabaseRepositories.Abstractions.Exceptions;
using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.DatabaseRepositories.Abstractions.Repositories.Exceptions;
using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using Firepuma.DatabaseRepositories.MongoDb.Queries;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Firepuma.DatabaseRepositories.MongoDb.Repositories;

public abstract class MongoDbRepository<T> : IRepository<T> where T : BaseMongoDbEntity
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

    private static string GenerateId() => ObjectId.GenerateNewId().ToString(); // generate and don't allow overwriting because BaseMongoDbEntity has BsonId for the Id field

    protected virtual string CollectionNameForLogs => Collection.CollectionNamespace.FullName;

    protected virtual string GenerateETag() => $"{DateTimeOffset.UtcNow:O}-{Guid.NewGuid().ToString()}";

    public async Task<IEnumerable<T>> GetItemsAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var queryable = ApplyQuery(querySpecification);

        var items = await queryable.ToListAsync(cancellationToken);

        Logger.LogInformation(
            "Fetched items in collection {Collection}",
            CollectionNameForLogs);

        return items;
    }

    public async Task<int> GetItemsCountAsync(IQuerySpecification<T> querySpecification, CancellationToken cancellationToken = default)
    {
        var queryable = ApplyQuery(querySpecification);

        var count = await queryable.CountAsync(cancellationToken);

        Logger.LogInformation(
            "Counted items in collection {Collection}",
            CollectionNameForLogs);

        return count;
    }

    public async Task<T?> GetItemOrDefaultAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await Collection
            .Find(i => i.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        Logger.LogInformation(
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

    public async Task<T> AddItemAsync(T item, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(item.Id))
        {
            throw new InvalidOperationException($"Item Id should not be specified when calling MongoDbRepository.AddItemAsync, it is auto-generated (item id {item.Id})");
        }

        item.Id = GenerateId();
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
        var oldETag = item.ETag;

        var options = new ReplaceOptions();

        Expression<Func<T, bool>> filter =
            ignoreETag
                ? i => i.Id == item.Id
                : i => i.Id == item.Id && i.ETag == oldETag;

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        item.Id ??= GenerateId();
        item.ETag = GenerateETag();

        Logger.LogInformation(
            "Upserted item id {Id} in collection {Collection}",
            item.Id, CollectionNameForLogs);

        var replaceResult = await Collection.ReplaceOneAsync(filter, item, options, cancellationToken);

        if (!ignoreETag)
        {
            VerifyReplacedExactlyOneDocument(options, replaceResult);
        }

        return item;
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